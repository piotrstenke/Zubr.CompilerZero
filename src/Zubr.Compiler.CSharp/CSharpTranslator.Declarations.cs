using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Zubr.Compiler.Syntax;
using Zubr.Compiler.Syntax.Abstractions;

using CSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSyntaxToken = Microsoft.CodeAnalysis.SyntaxToken;
using CSyntaxTokenList = Microsoft.CodeAnalysis.SyntaxTokenList;

using TokenList = Zubr.Compiler.Syntax.Abstractions.TokenList;

using Sharp = Microsoft.CodeAnalysis.CSharp.Syntax; 

namespace Zubr.Compiler.CSharp;

internal sealed partial class CSharpTranslator
{
	private static class Declarations
	{
		public static Sharp.BaseNamespaceDeclarationSyntax? Namespace(ModuleDeclarationSyntax node, bool hasMultipleModules)
		{
			if (node.Name is null)
			{
				// C# equivalent of 'module top;' is not declaring a namespace at all.
				return null;
			}

			Sharp.BaseNamespaceDeclarationSyntax @namespace = hasMultipleModules
				? SyntaxFactory.NamespaceDeclaration(Expressions.Name(node.Name))
				: SyntaxFactory.FileScopedNamespaceDeclaration(Expressions.Name(node.Name));

			List<Sharp.MemberDeclarationSyntax> targetMembers = new(node.Members.Count);
			List<Sharp.MethodDeclarationSyntax> globalFunctions = new();

			AddModuleMembers(node, targetMembers, globalFunctions);

			if (targetMembers.Count > 0)
			{
				@namespace = @namespace.WithMembers(SyntaxFactory.List(targetMembers));
			}

			if (globalFunctions.Count > 0)
			{
				@namespace = @namespace.AddMembers(GlobalFunctionContainerType(globalFunctions));
			}

			return @namespace;
		}


		public static Sharp.MemberDeclarationSyntax Member(MemberDeclarationSyntax node, TypeContext context)
		{
			return node switch
			{
				BaseTypeDeclarationSyntax t => Type(t),
				ImplementationDeclarationSyntax i => Extension(i),
				BaseFunctionDeclarationSyntax f => Method(f, context),
				PropertyDeclarationSyntax p => Property(p, context),
				IndexerDeclarationSyntax ind => Indexer(ind, context),
				FieldDeclarationSyntax f => Field(f),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.BaseTypeDeclarationSyntax Type(BaseTypeDeclarationSyntax node)
		{
			return node switch
			{
				ClassDeclarationSyntax c => Class(c),
				StructDeclarationSyntax s => Struct(s),
				TraitDeclarationSyntax t => Interface(t),
				SimpleEnumDeclarationSyntax e => Enum(e),
				AttributeDeclarationSyntax a => AttributeClass(a),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.ClassDeclarationSyntax AttributeClass(AttributeDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.TypeParameterList, node.ConstraintList);

			Sharp.ParameterListSyntax? parameters = node.ParameterList is null
				? null
				: ParameterList(node.ParameterList);

			Sharp.BaseListSyntax? baseTypeList = BaseTypeList(node.BaseTypeList);
			baseTypeList = AddBaseType(baseTypeList, Expressions.GlobalQualifiedName("System", "Attribute"));

			var attributes = Attributes(node.Attributes);

			if (HasFlag(flags, ModifierFlags.Limit))
			{
				attributes = attributes.Add(Interop.InternalInheritAttribute());
			}

			TypeContext context = new();
			var members = MembersIncludingParameters(node.Members, node.ParameterList, context);

			CheckDisposablePattern(node, modifiers, ref baseTypeList, ref members, context.Flags);
			CheckAddionalMembers(ref members, context);

			return SyntaxFactory.ClassDeclaration(
				attributes,
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text + "Attribute"),
				typeParameterList,
				parameters,
				baseTypeList,
				constraints,
				members
			);
		}

		public static Sharp.TypeDeclarationSyntax Class(ClassDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.TypeParameterList, node.ConstraintList);

			Sharp.ParameterListSyntax? parameters = node.ParameterList is null
				? null
				: ParameterList(node.ParameterList);

			var attributes = Attributes(node.Attributes);

			Sharp.BaseListSyntax? baseTypeList = BaseTypeList(node.BaseTypeList);

			if (HasFlag(flags, ModifierFlags.Limit))
			{
				attributes = attributes.Add(Interop.InternalInheritAttribute());
			}

			TypeContext context = new();
			var members = Members(node.Members, context);

			CheckDisposablePattern(node, modifiers, ref baseTypeList, ref members, context.Flags);
			CheckAddionalMembers(ref members, context);

			if (HasFlag(flags, ModifierFlags.Data))
			{
				return SyntaxFactory.RecordDeclaration(
					attributes,
					modifiers,
					default,
					SyntaxFactory.Identifier(node.Identifier.Text),
					typeParameterList,
					parameters,
					baseTypeList,
					constraints,
					members
				);
			}

			return SyntaxFactory.ClassDeclaration(
				attributes,
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				parameters,
				baseTypeList,
				constraints,
				members
			);
		}

		public static Sharp.ClassDeclarationSyntax Extension(ImplementationDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.TypeParameterList, node.ConstraintList);

			Sharp.ParameterListSyntax parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(
				SyntaxFactory.Parameter(
					default,
					default,
					Expressions.Type(node.Type),
					SyntaxFactory.Identifier("self"),
					default
			)));

			TypeContext context = new();
			var members = Members(node.Members, context);

			CheckAddionalMembers(ref members, context);

			Sharp.ExtensionBlockDeclarationSyntax block = SyntaxFactory.ExtensionBlockDeclaration(
				Attributes(node.Attributes),
				modifiers,
				typeParameterList,
				parameterList,
				constraints,
				members
			);

			CSyntaxTokenList classModifiers = GetAccessModifiers(modifiers)
				.Add(SyntaxFactory.Token(CSyntaxKind.StaticKeyword))
				.Add(SyntaxFactory.Token(CSyntaxKind.PartialKeyword));

			string identififer = GetIdentifier(node.Type) ?? string.Empty;

			return SyntaxFactory.ClassDeclaration(
				attributeLists: default,
				classModifiers,
				SyntaxFactory.Identifier($"{identififer}Extensions"),
				typeParameterList: default,
				parameterList: default,
				baseList: default,
				constraintClauses: default,
				SyntaxFactory.SingletonList<Sharp.MemberDeclarationSyntax>(block)
			);
		}

		public static Sharp.TypeDeclarationSyntax Struct(StructDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.TypeParameterList, node.ConstraintList);

			Sharp.ParameterListSyntax? parameters = node.ParameterList is null
				? null
				: ParameterList(node.ParameterList);

			Sharp.BaseListSyntax? baseTypeList = BaseTypeList(node.BaseTypeList);

			TypeContext context = new();

			var members = Members(node.Members, context);

			CheckDisposablePattern(node, modifiers, ref baseTypeList, ref members, context.Flags);
			CheckAddionalMembers(ref members, context);

			if (HasFlag(flags, ModifierFlags.Data))
			{
				return SyntaxFactory.RecordDeclaration(
					Attributes(node.Attributes),
					modifiers,
					SyntaxFactory.Token(CSyntaxKind.StructKeyword),
					SyntaxFactory.Identifier(node.Identifier.Text),
					typeParameterList,
					parameters,
					baseTypeList,
					constraints,
					members
				);
			}

			return SyntaxFactory.StructDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				baseTypeList,
				constraints,
				members
			);
		}

		public static Sharp.InterfaceDeclarationSyntax Interface(TraitDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.TypeParameterList, node.ConstraintList);

			TypeContext context = new();
			var members = Members(node.Members, context);

			CheckAddionalMembers(ref members, context);

			return SyntaxFactory.InterfaceDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				BaseTypeList(node.BaseTypeList),
				constraints,
				members
			);
		}

		public static Sharp.EnumDeclarationSyntax Enum(SimpleEnumDeclarationSyntax node)
		{
			// TODO: Implement enum struct and enum class.

			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			if(HasFlag(flags, ModifierFlags.Flag))
			{
				attributes.Add(Interop.FlagsAttribute());

				int count = 0;

				// TODO: Handle flags with initializer.
				return SyntaxFactory.EnumDeclaration(
					attributes,
					modifiers,
					SyntaxFactory.Identifier(node.Identifier.Text),
					BaseTypeList(node.BaseTypeList),
					SyntaxFactory.SeparatedList(node.Members
						.Cast<SimpleEnumMemberDeclarationSyntax>()
						.Select(x =>
						{
							if(count == 0)
							{
								return EnumMember(x, 0);
							}

							return EnumMember(x, 1 << count);
						})
				));
			}

			return SyntaxFactory.EnumDeclaration(
				attributes,
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				BaseTypeList(node.BaseTypeList),
				SyntaxFactory.SeparatedList(node.Members
					.Cast<SimpleEnumMemberDeclarationSyntax>()
					.Select(x => EnumMember(x))
			));
		}

		public static Sharp.FieldDeclarationSyntax Field(FieldDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			return SyntaxFactory.FieldDeclaration(
				Attributes(node.Attributes),
				modifiers,
				Variable(node.Variable)
			);
		}

		public static Sharp.PropertyDeclarationSyntax Property(PropertyDeclarationSyntax node, TypeContext context)
		{
			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			CheckBaseKeyword(node, ref attributes, ref modifiers, flags);

			if (node.ExpressionBody is not null)
			{
				return SyntaxFactory.PropertyDeclaration(
					attributes,
					modifiers,
					Expressions.Type(node.Type),
					null,
					SyntaxFactory.Identifier(node.Identifier.Text),
					null,
					Statements.ExpressionBody(node.ExpressionBody),
					null,
					SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
				);
			}

			if(node.AccessorList is null)
			{
				return SyntaxFactory.PropertyDeclaration(
					attributes,
					modifiers,
					Expressions.Type(node.Type),
					null,
					SyntaxFactory.Identifier(node.Identifier.Text),
					GenerateAccessorList(flags),
					null,
					Initializer(node.Initializer),
					SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
				);
			}

			AccessorDeclarationSyntax? getAccessorSource = GetAccessor(node, TokenKind.GetKeyword);
			AccessorDeclarationSyntax? setAccessorSource = GetAccessor(node, TokenKind.SetKeyword);

			// Zubr does not support set-only properties.
			Sharp.AccessorDeclarationSyntax getAccessor = getAccessorSource is null
				? SyntaxFactory.AccessorDeclaration(CSyntaxKind.GetAccessorDeclaration)
				: Accessor(getAccessorSource, CSyntaxKind.GetKeyword, context);

			Sharp.AccessorDeclarationSyntax? setAccessor = null;

			if (setAccessorSource is null)
			{
				if (HasFlag(flags, ModifierFlags.Mut))
				{
					setAccessor = SyntaxFactory.AccessorDeclaration(CSyntaxKind.SetAccessorDeclaration);
				}
				else if (HasFlag(flags, ModifierFlags.Init))
				{
					setAccessor = SyntaxFactory.AccessorDeclaration(CSyntaxKind.InitAccessorDeclaration);
				}
			}
			else
			{
				if (HasFlag(flags, ModifierFlags.Init))
				{
					setAccessor = Accessor(setAccessorSource, CSyntaxKind.InitKeyword, context);
				}
				else
				{
					setAccessor = Accessor(setAccessorSource, CSyntaxKind.SetKeyword, context);
				}
			}

			Sharp.AccessorListSyntax accessorList = setAccessor is null
				? SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(getAccessor))
				: SyntaxFactory.AccessorList(SyntaxFactory.List([getAccessor, setAccessor]));

			return SyntaxFactory.PropertyDeclaration(
				attributes,
				modifiers,
				Expressions.Type(node.Type),
				null,
				SyntaxFactory.Identifier(node.Identifier.Text),
				accessorList,
				null,
				Initializer(node.Initializer),
				node.Initializer is null ? default : SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
			);

			static AccessorDeclarationSyntax? GetAccessor(PropertyDeclarationSyntax node, TokenKind keyword)
			{
				return node.AccessorList!.Accessors.FirstOrDefault(x => x.Keyword.IsKind(keyword));
			}

			static Sharp.AccessorListSyntax GenerateAccessorList(ModifierFlags flags)
			{
				Sharp.AccessorDeclarationSyntax getAccessor = SyntaxFactory.AccessorDeclaration(CSyntaxKind.GetAccessorDeclaration);
				Sharp.AccessorListSyntax accessorList;

				if (HasFlag(flags, ModifierFlags.Mut))
				{
					accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List([
							getAccessor,
						SyntaxFactory.AccessorDeclaration(CSyntaxKind.SetAccessorDeclaration)
					]));
				}
				else if (HasFlag(flags, ModifierFlags.Init))
				{
					accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List([
						getAccessor,
						SyntaxFactory.AccessorDeclaration(CSyntaxKind.InitAccessorDeclaration)
					]));
				}
				else
				{
					accessorList = SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(getAccessor));
				}

				return accessorList;
			}
		}

		public static Sharp.IndexerDeclarationSyntax Indexer(IndexerDeclarationSyntax node, TypeContext context)
		{
			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			CheckBaseKeyword(node, ref attributes, ref modifiers, flags);

			if (node.AccessorList is null)
			{
				return SyntaxFactory.IndexerDeclaration(
					attributes,
					modifiers,
					Expressions.Type(node.Type),
					null,
					SyntaxFactory.Token(CSyntaxKind.ThisKeyword),
					ParameterList(node.ParameterList),
					null,
					Statements.ExpressionBody(node.ExpressionBody!),
					SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
				);
			}

			return SyntaxFactory.IndexerDeclaration(
				attributes,
				modifiers,
				Expressions.Type(node.Type),
				null,
				SyntaxFactory.Token(CSyntaxKind.ThisKeyword),
				ParameterList(node.ParameterList),
				AccessorList(node.AccessorList, HasFlag(flags, ModifierFlags.Init), context),
				null,
				default
			);
		}

		public static Sharp.VariableDeclarationSyntax Variable(VariableDeclarationSyntax node)
		{
			return SyntaxFactory.VariableDeclaration(Expressions.Type(node.Type),
				SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.VariableDeclarator(
						SyntaxFactory.Identifier(node.Identifier.Text),
						null,
						Initializer(node.Initializer)
				)));
		}

		public static Sharp.BaseMethodDeclarationSyntax Method(BaseFunctionDeclarationSyntax node, TypeContext context)
		{
			return node switch
			{
				FunctionDeclarationSyntax f => Method(f, context),
				ConstructorDeclarationSyntax c => Constructor(c, context),
				DestructorDeclarationSyntax d => DestructorMethod(d, context),
				CastDeclarationSyntax cast => ConversionOperator(cast, context),
				OperatorDeclarationSyntax o => Operator(o, context),
				InvokerDeclarationSyntax i => InvokeMethod(i, context),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.MethodDeclarationSyntax Method(FunctionDeclarationSyntax node, TypeContext context)
		{
			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			CheckBaseKeyword(node, ref attributes, ref modifiers, flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			Sharp.ParameterListSyntax parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(Parameter)));
			var constraints = ConstraintList(node.TypeParameterList, node.ConstraintList);

			return SyntaxFactory.MethodDeclaration(
				attributes,
				modifiers,
				Expressions.Type(node.ReturnType),
				default,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				parameters,
				constraints,
				node.Body is null ? null : Statements.Block(node.Body, context),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.ConstructorDeclarationSyntax Constructor(ConstructorDeclarationSyntax node, TypeContext context)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);
			CSyntaxToken identifier = node.Parent is TypeDeclarationSyntax parentType
				? SyntaxFactory.Identifier(parentType.Identifier.Text)
				: default;

			Sharp.ParameterListSyntax parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(Parameter)));

			// TODO: What about constructor initializers?
			return SyntaxFactory.ConstructorDeclaration(
				Attributes(node.Attributes),
				modifiers,
				identifier,
				parameters,
				null,
				node.Body is null ? null : Statements.Block(node.Body, context),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.MethodDeclarationSyntax DestructorMethod(DestructorDeclarationSyntax node, TypeContext context)
		{
			// TODO: Implement the IDisposable interface for the parent type.

			CSyntaxToken identifier;

			if(node.Keyword.IsKind(TokenKind.GCFreeKeyword))
			{
				identifier = SyntaxFactory.Identifier("free_unmanaged");
				context.AddFlag(TypeFlags.Destructor);
			}
			else
			{
				identifier = SyntaxFactory.Identifier("free_managed");
			}

			context.AddFlag(TypeFlags.Disposable);

			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			Sharp.ParameterListSyntax parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(Parameter)));

			return SyntaxFactory.MethodDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.PredefinedType(SyntaxFactory.Token(CSyntaxKind.VoidKeyword)),
				default,
				identifier,
				default,
				parameters,
				default,
				node.Body is null ? null : Statements.Block(node.Body, context),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.OperatorDeclarationSyntax Operator(OperatorDeclarationSyntax node, TypeContext context)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			return SyntaxFactory.OperatorDeclaration(
				Attributes(node.Attributes),
				modifiers,
				Expressions.Type(node.ReturnType),
				SyntaxFactory.Token(CSyntaxKind.OperatorKeyword),
				SyntaxFactory.Token(GetOperatorKind(node.OperatorToken.Kind)),
				ParameterList(node.ParameterList),
				node.Body is null ? null : Statements.Block(node.Body, context),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.ConversionOperatorDeclarationSyntax ConversionOperator(CastDeclarationSyntax node, TypeContext context)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			CSyntaxToken keyword = HasFlag(flags, ModifierFlags.Auto)
				? SyntaxFactory.Token(CSyntaxKind.ImplicitKeyword)
				: SyntaxFactory.Token(CSyntaxKind.ExplicitKeyword);

			return SyntaxFactory.ConversionOperatorDeclaration(
				Attributes(node.Attributes),
				modifiers,
				keyword,
				default,
				SyntaxFactory.Token(CSyntaxKind.OperatorKeyword),
				default, // TODO: What to do with checked/unchecked?
				Expressions.Type(node.Type),
				ParameterList(node.ParameterList),
				node.Body is null ? null : Statements.Block(node.Body, context),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.MethodDeclarationSyntax InvokeMethod(InvokerDeclarationSyntax node, TypeContext context)
		{
			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			CheckBaseKeyword(node, ref attributes, ref modifiers, flags);
			attributes = attributes.Add(Interop.InvokerAttribute());

			return SyntaxFactory.MethodDeclaration(
				attributes,
				modifiers,
				Expressions.Type(node.ReturnType),
				default,
				SyntaxFactory.Identifier("Invoke"),
				default,
				ParameterList(node.ParameterList),
				default,
				node.Body is null ? null : Statements.Block(node.Body, context),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.AttributeListSyntax AttributeList(AttributeSyntax node)
		{
			Sharp.AttributeArgumentListSyntax? args = node.ArgumentList is null
				? null
				: SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(node.ArgumentList.Arguments.Select(x =>
					SyntaxFactory.AttributeArgument(
						Expressions.Expression(x.Expression)))));

			return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
				SyntaxFactory.Attribute(
					Expressions.Name(node.Name),
					args
			)));
		}

		public static Microsoft.CodeAnalysis.SyntaxList<Sharp.AttributeListSyntax> Attributes(Syntax.Abstractions.SyntaxList<AttributeSyntax> list)
		{
			if(list.IsDefaultOrEmpty)
			{
				return SyntaxFactory.List<Sharp.AttributeListSyntax>();
			}

			return SyntaxFactory.List(list.Select(AttributeList));
		}

		public static Sharp.BracketedParameterListSyntax ParameterList(BracketParameterListSyntax node)
		{
			return SyntaxFactory.BracketedParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(Parameter)));
		}

		public static Sharp.ParameterListSyntax ParameterList(ParameterListSyntax node)
		{
			return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(Parameter)));
		}

		public static Sharp.ParameterSyntax Parameter(ParameterSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, node.Modifiers, out _);

			Sharp.EqualsValueClauseSyntax? defaultClause = node.Default is null
				? null
				: SyntaxFactory.EqualsValueClause(Expressions.Expression(node.Default.Value));

			return SyntaxFactory.Parameter(
				Attributes(node.Attributes),
				modifiers,
				Expressions.Type(node.Type),
				SyntaxFactory.Identifier(node.Identifier.Text),
				defaultClause
			);
		}

		[return: NotNullIfNotNull(nameof(node))]
		public static Sharp.TypeParameterListSyntax? TypeParameterList(TypeParameterListSyntax? node)
		{
			if (node is null)
			{
				return null;
			}

			return SyntaxFactory.TypeParameterList(SyntaxFactory.SeparatedList(node.Parameters.Select(TypeParameter)));
		}

		public static Sharp.TypeParameterSyntax TypeParameter(TypeParameterSyntax node)
		{
			var attributes = Attributes(node.Attributes);

			if(node.DefaultType is not null)
			{
				attributes = attributes.Add(Interop.DefaultTypeParameterAttribute(node.DefaultType.Type));
			}

			return SyntaxFactory.TypeParameter(
				attributes,
				default,
				SyntaxFactory.Identifier(node.Identifier.Text)
			);
		}

		public static Microsoft.CodeAnalysis.SyntaxList<Sharp.TypeParameterConstraintClauseSyntax> ConstraintList(TypeParameterListSyntax? typeParameterList, TypeParameterConstraintListSyntax? constraintList)
		{
			if(typeParameterList is null)
			{
				return default;
			}

			List<Sharp.TypeParameterConstraintClauseSyntax> clauses = new(typeParameterList.Parameters.Count);

			foreach (TypeParameterSyntax typeParameter in typeParameterList.Parameters)
			{
				if (typeParameter.InlineConstraint is null)
				{
					continue;
				}

				ConstraintFlags flags = default;
				Sharp.TypeParameterConstraintSyntax constraint = Constraint(typeParameter.InlineConstraint.Constraint, ref flags);
				Sharp.TypeParameterConstraintSyntax? additionalConstraint = GetAdditionalConstraint(flags);

				if (additionalConstraint is null)
				{
					clauses.Add(SyntaxFactory.TypeParameterConstraintClause(
						SyntaxFactory.IdentifierName(typeParameter.Identifier.Text),
						SyntaxFactory.SingletonSeparatedList(constraint)));
				}
				else
				{

					clauses.Add(SyntaxFactory.TypeParameterConstraintClause(
						SyntaxFactory.IdentifierName(typeParameter.Identifier.Text),
						SyntaxFactory.SeparatedList([constraint, additionalConstraint])));
				}
			}

			if (constraintList is not null)
			{
				foreach (TypeParameterConstraintClauseSyntax clause in constraintList.Clauses)
				{
					ConstraintFlags flags = default;

					List<Sharp.TypeParameterConstraintSyntax> constraints = new(clause.Constraints.Count);

					foreach (TypeParameterConstraintSyntax constraint in clause.Constraints)
					{
						constraints.Add(Constraint(constraint, ref flags));
					}

					Sharp.TypeParameterConstraintSyntax? additionalConstraint = GetAdditionalConstraint(flags);

					if(additionalConstraint is not null)
					{
						constraints.Add(additionalConstraint);
					}

					clauses.Add(SyntaxFactory.TypeParameterConstraintClause(
						SyntaxFactory.IdentifierName(clause.Identifier.Text),
						SyntaxFactory.SeparatedList(constraints
							// class and struct constraints must be defined first in C#
							.OrderBy(x => x is Sharp.ClassOrStructConstraintSyntax ? 0 : 1)
						)
					));
				}
			}

			return SyntaxFactory.List(clauses);

			static Sharp.TypeParameterConstraintSyntax Constraint(TypeParameterConstraintSyntax constraint, ref ConstraintFlags flags)
			{
				return constraint switch
				{
					KeywordConstraintSyntax c => KeywordConstraint(c, ref flags),
					TypeConstraintSyntax t => SyntaxFactory.TypeConstraint(Expressions.Type(t.Type)),
					_ => throw new UnreachableException()
				};
			}

			static Sharp.TypeParameterConstraintSyntax KeywordConstraint(KeywordConstraintSyntax constraint, ref ConstraintFlags flags)
			{
				switch(constraint.Kind)
				{
					case SyntaxKind.ClassConstraint:
						return SyntaxFactory.ClassOrStructConstraint(
							CSyntaxKind.ClassConstraint,
							SyntaxFactory.Token(CSyntaxKind.ClassKeyword),
							constraint.QuestionToken.IsAny
								? SyntaxFactory.Token(CSyntaxKind.QuestionToken) 
								: default
						);

					case SyntaxKind.StructConstraint:

						flags |= ConstraintFlags.StructOrUnmanaged;

						return SyntaxFactory.ClassOrStructConstraint(
							CSyntaxKind.StructConstraint,
							SyntaxFactory.Token(CSyntaxKind.StructKeyword),
							constraint.QuestionToken.IsAny
								? SyntaxFactory.Token(CSyntaxKind.QuestionToken) 
								: default
						);

					case SyntaxKind.EnumConstraint:

						flags |= ConstraintFlags.Enum;

						return SyntaxFactory.TypeConstraint(Expressions.GlobalQualifiedName("System", "Enum"));

					case SyntaxKind.UnmanagedConstraint:

						flags |= ConstraintFlags.StructOrUnmanaged;

						return SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("unmanaged")));

					default:
						throw new UnreachableException();
				}
			}

			static Sharp.TypeParameterConstraintSyntax? GetAdditionalConstraint(ConstraintFlags flags)
			{
				if (flags.HasFlag(ConstraintFlags.Enum) && !flags.HasFlag(ConstraintFlags.StructOrUnmanaged))
				{
					return SyntaxFactory.ClassOrStructConstraint(CSyntaxKind.StructConstraint);
				}

				return null;
			}
		}

		public static CSyntaxTokenList GetModifiers(MemberDeclarationSyntax node, out ModifierFlags flags)
		{
			return GetModifiers(node, node.Modifiers, out flags);
		}

		public static CSyntaxTokenList GetModifiers(SyntaxNode node, TokenList modifiers, out ModifierFlags flags)
		{
			List<CSyntaxToken> targetModifiers = new(modifiers.Count);

			flags = ModifierFlags.None;

			bool isOpen = false;
			bool hasAccessModifiers = false;
			bool isStatic = false;

			for (int i = 0; i < modifiers.Count; i++)
			{
				TokenKind kind = modifiers[i].Kind;

				switch(kind)
				{
					case TokenKind.PrivKeyword:
						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PrivateKeyword));
						hasAccessModifiers = true;
						break;

					case TokenKind.ProtKeyword:
						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.ProtectedKeyword));
						hasAccessModifiers = true;
						break;

					case TokenKind.ScopedKeyword:
						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.InternalKeyword));
						hasAccessModifiers = true;
						break;

					case TokenKind.PubKeyword:

						// Destructor method in C# must not be public.
						if (node is DestructorDeclarationSyntax)
						{
							continue;
						}

						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PublicKeyword));
						hasAccessModifiers = true;

						break;

					case TokenKind.OverKeyword:

						if (!hasAccessModifiers)
						{
							targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PublicKeyword));
						}

						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.OverrideKeyword));
						break;

					case TokenKind.ReqKeyword:

						if (!hasAccessModifiers)
						{
							targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PublicKeyword));
						}

						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.RequiredKeyword));
						break;

					case TokenKind.ConstKeyword:

						if (!hasAccessModifiers)
						{
							targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PublicKeyword));
						}

						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.ConstKeyword));
						break;

					case TokenKind.BaseKeyword:
						
						if(node is TypeDeclarationSyntax)
						{
							targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.AbstractKeyword));
						}
						else
						{
							// Base keyword is handled by the property/function.
							flags |= ModifierFlags.Base;
						}

						isOpen = true;

						break;

					case TokenKind.LocalKeyword:
						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.RefKeyword));
						break;

					case TokenKind.UnsafeKeyword:
						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.UnsafeKeyword));
						break;

					case TokenKind.OpenKeyword:
						isOpen = true;
						break;

					case TokenKind.StatKeyword:
						isStatic = true;
						break;

					case TokenKind.AutoKeyword:
						flags |= ModifierFlags.Auto;
						break;

					case TokenKind.MutKeyword:
						flags |= ModifierFlags.Mut;
						break;

					case TokenKind.LimitKeyword:
						flags |= ModifierFlags.Limit;
						break;

					case TokenKind.DataKeyword:
						flags |= ModifierFlags.Data;
						break;

					case TokenKind.FlagKeyword:
						flags |= ModifierFlags.Flag;
						break;

					// C# determines whether a struct is managed/unmanaged without any keywords.
					case TokenKind.ManagedKeyword:
					case TokenKind.UnmanagedKeyword:
						continue;
				}
			}

			if (isOpen)
			{
				if(node is FunctionDeclarationSyntax)
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.VirtualKeyword));
				}
			}
			else
			{
				// Classes in C# are 'open' by default.
				if (node is ClassDeclarationSyntax or AttributeDeclarationSyntax)
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.SealedKeyword));
				}
			}

			if(node is FieldDeclarationSyntax)
			{
				if(!hasAccessModifiers)
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PrivateKeyword));
				}

				if(!HasFlag(flags, ModifierFlags.Mut))
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.ReadOnlyKeyword));
				}
			}
			else if(node is DestructorDeclarationSyntax)
			{
				targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PrivateKeyword));
			}
			else if(node is StructDeclarationSyntax)
			{
				if(!flags.HasFlag(ModifierFlags.Mut))
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.ReadOnlyKeyword));
				}
			}

			if (!isStatic && ShouldAddStatic(node))
			{
				targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.StaticKeyword));
			}

			return SyntaxFactory.TokenList(targetModifiers.ToArray());

			static bool ShouldAddStatic(SyntaxNode node)
			{
				if(node is FunctionDeclarationSyntax)
				{
					return node.Parent is CompilationUnitSyntax or ModuleDeclarationSyntax;
				}

				if(node is CastDeclarationSyntax)
				{
					return true;
				}

				return false;
			}
		}

		private static Microsoft.CodeAnalysis.SyntaxList<Sharp.MemberDeclarationSyntax> Members(Syntax.Abstractions.SyntaxList<MemberDeclarationSyntax> members, TypeContext context)
		{
			if(members.IsDefaultOrEmpty)
			{
				return default;
			}

			List<Sharp.MemberDeclarationSyntax> list = new(members.Count);

			foreach (MemberDeclarationSyntax member in members)
			{
				list.Add(Member(member, context));
			}

			return SyntaxFactory.List(list);
		}

		private static Microsoft.CodeAnalysis.SyntaxList<Sharp.MemberDeclarationSyntax> MembersIncludingParameters(Syntax.Abstractions.SyntaxList<MemberDeclarationSyntax> members, ParameterListSyntax? parameterList, TypeContext context)
		{
			if (parameterList is null || parameterList.Parameters.Count == 0)
			{
				return Members(members, context);
			}

			List<Sharp.MemberDeclarationSyntax> list;

			if (members.IsDefaultOrEmpty)
			{
				list = new(parameterList.Parameters.Count);
			}
			else
			{
				list = new(members.Count + parameterList.Parameters.Count);
			}
		
			foreach (ParameterSyntax parameter in parameterList.Parameters)
			{
				list.Add(SyntaxFactory.PropertyDeclaration(
					default,
					SyntaxFactory.TokenList(SyntaxFactory.Token(CSyntaxKind.PublicKeyword)),
					Expressions.Type(parameter.Type),
					default,
					SyntaxFactory.Identifier(parameter.Identifier.Text),
					SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(SyntaxFactory.AccessorDeclaration(CSyntaxKind.GetAccessorDeclaration))),
					default,
					SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(parameter.Identifier.Text)),
					SyntaxFactory.Token(CSyntaxKind.SemicolonToken))
				);
			}

			if (!members.IsDefault)
			{
				foreach (MemberDeclarationSyntax member in members)
				{
					list.Add(Member(member, context));
				}
			}

			return SyntaxFactory.List(list);
		}


		private static Sharp.EnumMemberDeclarationSyntax EnumMember(SimpleEnumMemberDeclarationSyntax node)
		{
			return SyntaxFactory.EnumMemberDeclaration(
				Attributes(node.Attributes),
				SyntaxFactory.Identifier(node.Identifier.Text),
				Initializer(node.Initializer)
			);
		}

		private static Sharp.EnumMemberDeclarationSyntax EnumMember(SimpleEnumMemberDeclarationSyntax node, int value)
		{
			return SyntaxFactory.EnumMemberDeclaration(
				Attributes(node.Attributes),
				SyntaxFactory.Identifier(node.Identifier.Text),
				SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(CSyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value)))
			);
		}

		private static void CheckBaseKeyword(BaseFunctionDeclarationSyntax node, ref Microsoft.CodeAnalysis.SyntaxList<Sharp.AttributeListSyntax> attributes, ref CSyntaxTokenList modifiers, ModifierFlags flags)
		{
			if (HasFlag(flags, ModifierFlags.Base))
			{
				if (node.ExpressionBody is null && node.Body is null)
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.AbstractKeyword));
				}
				else
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.VirtualKeyword));
					attributes = attributes.Add(Interop.MustOverrideAttribute());
				}
			}
		}

		private static void CheckBaseKeyword(BasePropertyDeclarationSyntax node, ref Microsoft.CodeAnalysis.SyntaxList<Sharp.AttributeListSyntax> attributes, ref CSyntaxTokenList modifiers, ModifierFlags flags)
		{
			if (HasFlag(flags, ModifierFlags.Base))
			{
				if (node.ExpressionBody is null && !HasAccessorWithImplementation(node.AccessorList))
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.AbstractKeyword));
				}
				else
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.VirtualKeyword));
					attributes = attributes.Add(Interop.MustOverrideAttribute());
				}
			}

			static bool HasAccessorWithImplementation(AccessorListSyntax? node)
			{
				if(node is null)
				{
					return false;
				}

				foreach (AccessorDeclarationSyntax accessor in node.Accessors)
				{
					if(accessor.ExpressionBody is not null || accessor.Block is not null)
					{
						return true;
					}
				}

				return false;
			}
		}

		private static void CheckDisposablePattern(
			TypeDeclarationSyntax node,
			CSyntaxTokenList modifiers,
			ref Sharp.BaseListSyntax? baseTypeList,
			ref Microsoft.CodeAnalysis.SyntaxList<Sharp.MemberDeclarationSyntax> members,
			TypeFlags typeFlags
		)
		{
			if (HasFlag(typeFlags, TypeFlags.Disposable))
			{
				if (baseTypeList is null)
				{
					baseTypeList = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<Sharp.BaseTypeSyntax>(Interop.ImplementIDisposable()));
				}
				else
				{
					baseTypeList = baseTypeList.AddTypes(Interop.ImplementIDisposable());
				}

				bool isOpen = node is ClassDeclarationSyntax && !modifiers.Any(CSyntaxKind.SealedKeyword);

				members = members.AddRange(Interop.DisposablePattern(isOpen, HasFlag(typeFlags, TypeFlags.Destructor)));
			}

			if (HasFlag(typeFlags, TypeFlags.Destructor))
			{
				members = members.Add(Interop.IDisposableDestructor(SyntaxFactory.Identifier(node.Identifier.Text)));
			}
		}

		private static Sharp.AccessorListSyntax AccessorList(AccessorListSyntax node, bool isInit, TypeContext context)
		{
			return SyntaxFactory.AccessorList(SyntaxFactory.List(node.Accessors.Select(x => Accessor(x, x.Kind switch
			{
				SyntaxKind.GetAccessorDeclaration => CSyntaxKind.GetKeyword,
				SyntaxKind.SetAccessorDeclaration => isInit
					? CSyntaxKind.InitKeyword
					: CSyntaxKind.SetKeyword,
				_ => throw new UnreachableException()
			}, context))));
		}

		private static Sharp.AccessorDeclarationSyntax Accessor(AccessorDeclarationSyntax node, CSyntaxKind kind, TypeContext context)
		{
			CSyntaxKind decl = Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetAccessorDeclarationKind(kind);

			CSyntaxTokenList modifiers = GetModifiers(node, node.Modifiers, out _);

			if (node.Block is not null)
			{
				return SyntaxFactory.AccessorDeclaration(decl,
					Attributes(node.Attributes),
					modifiers,
					SyntaxFactory.Token(kind),
					Statements.Block(node.Block, context),
					null,
					SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
				);
			}

			if (node.ExpressionBody is not null)
			{
				return SyntaxFactory.AccessorDeclaration(decl,
					Attributes(node.Attributes),
					modifiers,
					SyntaxFactory.Token(kind),
					null,
					Statements.ExpressionBody(node.ExpressionBody),
					SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
				);
			}

			return SyntaxFactory.AccessorDeclaration(decl,
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Token(kind),
				null,
				null,
				SyntaxFactory.Token(CSyntaxKind.SemicolonToken)
			);
		}

		[return: NotNullIfNotNull(nameof(node))]
		private static Sharp.BaseListSyntax? BaseTypeList(BaseTypeListSyntax? node)
		{
			if (node is null)
			{
				return null;
			}

			return SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(node.Types.Select<BaseTypeSyntax, Sharp.BaseTypeSyntax>(x => x switch
			{
				SimpleBaseTypeSyntax s => SyntaxFactory.SimpleBaseType(Expressions.Type(x.Type)),
				PrimaryBaseTypeSyntax p => SyntaxFactory.PrimaryConstructorBaseType(Expressions.Type(p.Type)),
				_ => throw new UnreachableException()
			})));
		}

		private static Sharp.BaseListSyntax AddBaseType(Sharp.BaseListSyntax? node, Sharp.TypeSyntax type)
		{
			if(node is null)
			{
				return SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<Sharp.BaseTypeSyntax>(SyntaxFactory.SimpleBaseType(type)));
			}

			var baseTypes = node.Types;
			return node.WithTypes(baseTypes.Insert(0, SyntaxFactory.SimpleBaseType(type)));
		}

		[return: NotNullIfNotNull(nameof(node))]
		private static Sharp.EqualsValueClauseSyntax? Initializer(EqualsValueClauseSyntax? node)
		{
			if (node is null)
			{
				return null;
			}

			return SyntaxFactory.EqualsValueClause(Expressions.Expression(node.Value));
		}

		private static CSyntaxKind GetOperatorKind(TokenKind kind)
		{
			return kind switch
			{
				TokenKind.EqualsEqualsToken => CSyntaxKind.EqualsEqualsToken,
				TokenKind.EqualsGreaterThanToken => CSyntaxKind.EqualsGreaterThanToken,
				TokenKind.ExclamationToken => CSyntaxKind.ExclamationToken,
				TokenKind.ExclamationEqualsToken => CSyntaxKind.ExclamationEqualsToken,
				TokenKind.GreaterThanToken => CSyntaxKind.GreaterThanToken,
				TokenKind.GreaterThanGreaterThanToken => CSyntaxKind.GreaterThanGreaterThanToken,
				TokenKind.GreaterThanGreaterThanGreaterThanToken => CSyntaxKind.GreaterThanGreaterThanGreaterThanToken,
				TokenKind.GreaterThanEqualsToken => CSyntaxKind.GreaterThanEqualsToken,
				TokenKind.GreaterThanGreaterThanEqualsToken => CSyntaxKind.GreaterThanGreaterThanEqualsToken,
				TokenKind.GreaterThanGreaterThanGreaterThanEqualsToken => CSyntaxKind.GreaterThanGreaterThanGreaterThanEqualsToken,
				TokenKind.LessThanToken => CSyntaxKind.LessThanToken,
				TokenKind.LessThanLessThanToken => CSyntaxKind.LessThanLessThanToken,
				TokenKind.LessThanEqualsToken => CSyntaxKind.LessThanEqualsToken,
				TokenKind.LessThanLessThanEqualsToken => CSyntaxKind.LessThanLessThanEqualsToken,
				TokenKind.PlusToken => CSyntaxKind.PlusToken,
				TokenKind.PlusPlusToken => CSyntaxKind.PlusPlusToken,
				TokenKind.PlusEqualsToken => CSyntaxKind.PlusEqualsToken,
				TokenKind.MinusToken => CSyntaxKind.MinusToken,
				TokenKind.MinusMinusToken => CSyntaxKind.MinusMinusToken,
				TokenKind.MinusEqualsToken => CSyntaxKind.MinusEqualsToken,
				TokenKind.AsteriskToken => CSyntaxKind.AsteriskToken,
				TokenKind.AsteriskEqualsToken => CSyntaxKind.AsteriskEqualsToken,
				TokenKind.PercentToken => CSyntaxKind.PercentToken,
				TokenKind.PercentEqualsToken => CSyntaxKind.PercentEqualsToken,
				TokenKind.CaretToken => CSyntaxKind.CaretToken,
				TokenKind.CaretEqualsToken => CSyntaxKind.CaretEqualsToken,
				TokenKind.BarToken => CSyntaxKind.BarToken,
				TokenKind.BarEqualsToken => CSyntaxKind.BarEqualsToken,
				TokenKind.SlashToken => CSyntaxKind.SlashToken,
				TokenKind.SlashEqualsToken => CSyntaxKind.SlashEqualsToken,
				TokenKind.AmpersandToken => CSyntaxKind.AmpersandToken,
				TokenKind.AmpersandEqualsToken => CSyntaxKind.AmpersandEqualsToken,
				TokenKind.TildeToken => CSyntaxKind.TildeToken,
				TokenKind.TrueKeyword => CSyntaxKind.TrueKeyword,
				TokenKind.FalseKeyword => CSyntaxKind.FalseKeyword,
				_ => throw new UnreachableException()
			};
		}

		private static string? GetIdentifier(TypeSyntax node)
		{
			return node switch
			{
				NullableTypeSyntax n => GetIdentifier(n.ElementType),
				PredefinedTypeSyntax p => GetIdentifier(p),
				ArrayTypeSyntax a => GetIdentifier(a.ElementType),
				ReferenceTypeSyntax r => GetIdentifier(r.ElementType),
				PointerTypeSyntax p => GetIdentifier(p.ElementType),
				NameSyntax name => GetIdentifier(name),
				_ => null
			};
		}

		private static string GetIdentifier(PredefinedTypeSyntax node)
		{
			return node.Keyword.Kind switch
			{
				TokenKind.IntKeyword => "Int32",
				TokenKind.UIntKeyword => "UInt32",
				TokenKind.LongKeyword => "Int64",
				TokenKind.ULongKeyword => "UInt64",
				TokenKind.ShortKeyword => "Int16",
				TokenKind.UShortKeyword => "UInt16",
				TokenKind.BoolKeyword => "Boolean",
				TokenKind.StringKeyword => "String",
				TokenKind.CharKeyword => "Char",
				TokenKind.FloatKeyword => "Single",
				TokenKind.DoubleKeyword => "Double",
				TokenKind.HalfKeyword => "Half",
				TokenKind.DecimalKeyword => "Decimal",
				TokenKind.AnyKeyword => "Object",
				TokenKind.NIntKeyword => "IntPtr",
				TokenKind.NUIntKeyword => "UIntPtr",
				TokenKind.VoidKeyword => "Void",
				TokenKind.ByteKeyword => "Byte",
				TokenKind.SByteKeyword => "SByte",
				_ => throw new UnreachableException()
			};
		}

		private static string GetIdentifier(NameSyntax node)
		{
			return node switch
			{
				IdentifierNameSyntax i => i.Identifier.Text,
				GenericNameSyntax g => g.Identifier.Text,
				QualifiedNameSyntax q => q.Right.Identifier.Text,
				TopQualifiedNameSyntax t => t.Name.Identifier.Text,
				_ => throw new UnreachableException()
			};
		}

		private static void CheckAddionalMembers(ref Microsoft.CodeAnalysis.SyntaxList<Sharp.MemberDeclarationSyntax> members, TypeContext context)
		{
			if (!context.HasAddedMembers)
			{
				return;
			}

			members = members.AddRange(context.GetAddedMembers());
		}

		private static CSyntaxTokenList GetAccessModifiers(CSyntaxTokenList modifiers)
		{
			bool hasPrivate = false;
			bool hasProtected = false;
			bool hasInternal = false;

			CSyntaxToken previous = default;

			foreach (CSyntaxToken modifier in modifiers)
			{
				switch (modifier.Kind())
				{
					case CSyntaxKind.PrivateKeyword:

						if (hasProtected)
						{
							return SyntaxFactory.TokenList(modifier, previous);
						}

						hasPrivate = true;
						previous = modifier;
						break;

					case CSyntaxKind.ProtectedKeyword:

						if (hasPrivate)
						{
							return SyntaxFactory.TokenList(previous, modifier);
						}

						if (hasInternal)
						{
							return SyntaxFactory.TokenList(modifier, previous);
						}

						hasProtected = true;
						previous = modifier;
						break;

					case CSyntaxKind.InternalKeyword:

						if (hasProtected)
						{
							return SyntaxFactory.TokenList(previous, modifier);
						}

						hasInternal = true;
						previous = modifier;
						break;

					case CSyntaxKind.PublicKeyword:
					case CSyntaxKind.FileKeyword:
						return SyntaxFactory.TokenList(modifier);
				}
			}

			return previous == default ? default : SyntaxFactory.TokenList(previous);
		}
	}

	private static bool HasFlag(ModifierFlags flags, ModifierFlags target)
	{
		if (flags == default)
		{
			return false;
		}

		return (flags & target) == target;
	}

	private static bool HasFlag(TypeFlags flags, TypeFlags target)
	{
		if (flags == default)
		{
			return false;
		}

		return (flags & target) == target;
	}

	public sealed class TypeContext
	{
		private List<Sharp.MemberDeclarationSyntax>? _addedMembers;

		private TypeFlags _flags;

		public TypeFlags Flags { get; }

		public bool HasAddedMembers => _addedMembers is not null && _addedMembers.Count > 0;

		public void AddFlag(TypeFlags flag)
		{
			_flags |= flag;
		}

		public bool HasFlag(TypeFlags flag)
		{
			return _flags.HasFlag(flag);
		}

		public void AddMember(Sharp.MemberDeclarationSyntax member)
		{
			_addedMembers ??= new();
			_addedMembers.Add(member);
		}

		internal IEnumerable<Sharp.MemberDeclarationSyntax> GetAddedMembers()
		{
			return _addedMembers ?? [];
		}
	}

	[Flags]
	public enum ModifierFlags
	{
		None = 0,

		Mut = 1,

		Limit = 2,

		Base = 4,

		Flag = 8,

		Data = 16,

		Init = 32,

		Auto = 64,
	}

	[Flags]
	public enum TypeFlags
	{
		None = 0,

		Disposable = 1,

		Destructor = 2,
	}

	[Flags]
	private enum ConstraintFlags
	{
		None = 0,

		StructOrUnmanaged = 1,

		Enum = 2
	}
}
