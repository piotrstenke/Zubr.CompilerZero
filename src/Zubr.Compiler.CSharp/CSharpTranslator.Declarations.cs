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


		public static Sharp.MemberDeclarationSyntax Member(MemberDeclarationSyntax node, ref TypeFlags typeFlags)
		{
			return node switch
			{
				ClassDeclarationSyntax c => Class(c),
				StructDeclarationSyntax s => Struct(s),
				TraitDeclarationSyntax t => Interface(t),
				ImplementationDeclarationSyntax i => Extension(i),
				BaseFunctionDeclarationSyntax f => Method(f, ref typeFlags),
				PropertyDeclarationSyntax p => Property(p),
				IndexerDeclarationSyntax ind => Indexer(ind),
				SimpleEnumDeclarationSyntax e => Enum(e),
				AttributeDeclarationSyntax a => AttributeClass(a),
				FieldDeclarationSyntax f => Field(f),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.ClassDeclarationSyntax AttributeClass(AttributeDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.ConstraintList);

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

			TypeFlags typeFlags = default;
			var members = Members(node.Members, ref typeFlags);

			CheckDisposablePattern(node, modifiers, ref baseTypeList, ref members, typeFlags);

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
			var constraints = ConstraintList(node.ConstraintList);

			Sharp.ParameterListSyntax? parameters = node.ParameterList is null
				? null
				: ParameterList(node.ParameterList);

			var attributes = Attributes(node.Attributes);

			Sharp.BaseListSyntax? baseTypeList = BaseTypeList(node.BaseTypeList);

			if (HasFlag(flags, ModifierFlags.Limit))
			{
				attributes = attributes.Add(Interop.InternalInheritAttribute());
			}

			TypeFlags typeFlags = default;
			var members = Members(node.Members, ref typeFlags);

			CheckDisposablePattern(node, modifiers, ref baseTypeList, ref members, typeFlags);

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

		public static Sharp.ExtensionBlockDeclarationSyntax Extension(ImplementationDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.ConstraintList);

			Sharp.ParameterListSyntax parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(
				SyntaxFactory.Parameter(
					default,
					default,
					Expressions.Type(node.Type),
					SyntaxFactory.Identifier("self"),
					default
			)));

			return SyntaxFactory.ExtensionBlockDeclaration(
				Attributes(node.Attributes),
				modifiers,
				typeParameterList,
				parameterList,
				constraints,
				Members(node.Members)
			);
		}

		public static Sharp.TypeDeclarationSyntax Struct(StructDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.ConstraintList);

			Sharp.ParameterListSyntax? parameters = node.ParameterList is null
				? null
				: ParameterList(node.ParameterList);

			Sharp.BaseListSyntax? baseTypeList = BaseTypeList(node.BaseTypeList);

			TypeFlags typeFlags = default;
			var members = Members(node.Members, ref typeFlags);

			CheckDisposablePattern(node, modifiers, ref baseTypeList, ref members, typeFlags);

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
			var constraints = ConstraintList(node.ConstraintList);

			return SyntaxFactory.InterfaceDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				BaseTypeList(node.BaseTypeList),
				constraints,
				Members(node.Members)
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

		public static Sharp.PropertyDeclarationSyntax Property(PropertyDeclarationSyntax node)
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
				: Accessor(getAccessorSource, CSyntaxKind.GetKeyword);

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
					setAccessor = Accessor(setAccessorSource, CSyntaxKind.InitKeyword);
				}
				else
				{
					setAccessor = Accessor(setAccessorSource, CSyntaxKind.SetKeyword);
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

		public static Sharp.IndexerDeclarationSyntax Indexer(IndexerDeclarationSyntax node)
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
				AccessorList(node.AccessorList, HasFlag(flags, ModifierFlags.Init)),
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

		public static Sharp.BaseMethodDeclarationSyntax Method(BaseFunctionDeclarationSyntax node, ref TypeFlags typeFlags)
		{
			return node switch
			{
				FunctionDeclarationSyntax f => Method(f),
				ConstructorDeclarationSyntax c => Constructor(c),
				DestructorDeclarationSyntax d => DestructorMethod(d, ref typeFlags),
				CastDeclarationSyntax cast => ConversionOperator(cast),
				OperatorDeclarationSyntax o => Operator(o),
				InvokerDeclarationSyntax i => InvokeMethod(i),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.MethodDeclarationSyntax Method(FunctionDeclarationSyntax node)
		{
			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			CheckBaseKeyword(node, ref attributes, ref modifiers, flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			Sharp.ParameterListSyntax parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(Parameter)));
			var constraints = ConstraintList(node.ConstraintList);

			return SyntaxFactory.MethodDeclaration(
				attributes,
				modifiers,
				Expressions.Type(node.ReturnType),
				default,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				parameters,
				constraints,
				node.Body is null ? null : Statements.Block(node.Body),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.ConstructorDeclarationSyntax Constructor(ConstructorDeclarationSyntax node)
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
				node.Body is null ? null : Statements.Block(node.Body),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.MethodDeclarationSyntax DestructorMethod(DestructorDeclarationSyntax node, ref TypeFlags typeFlags)
		{
			// TODO: Implement the IDisposable interface for the parent type.

			CSyntaxToken identifier;

			if(node.Keyword.IsKind(TokenKind.GCFreeKeyword))
			{
				identifier = SyntaxFactory.Identifier("free_unmanaged");
				typeFlags |= TypeFlags.Destructor;
			}
			else
			{
				identifier = SyntaxFactory.Identifier("free_managed");
			}

			typeFlags |= TypeFlags.Disposable;

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
				node.Body is null ? null : Statements.Block(node.Body),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.OperatorDeclarationSyntax Operator(OperatorDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out _);

			return SyntaxFactory.OperatorDeclaration(
				Attributes(node.Attributes),
				modifiers,
				Expressions.Type(node.ReturnType),
				SyntaxFactory.Token(CSyntaxKind.OperatorKeyword),
				SyntaxFactory.Token(GetOperatorKind(node.OperatorToken.Kind)),
				ParameterList(node.ParameterList),
				node.Body is null ? null : Statements.Block(node.Body),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.ConversionOperatorDeclarationSyntax ConversionOperator(CastDeclarationSyntax node)
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
				node.Body is null ? null : Statements.Block(node.Body),
				node.ExpressionBody is null ? null : Statements.ExpressionBody(node.ExpressionBody),
				node.Body is null && node.ExpressionBody is null ? SyntaxFactory.Token(CSyntaxKind.SemicolonToken) : default
			);
		}

		public static Sharp.MethodDeclarationSyntax InvokeMethod(InvokerDeclarationSyntax node)
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
				node.Body is null ? null : Statements.Block(node.Body),
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
			return SyntaxFactory.TypeParameter(SyntaxFactory.Identifier(node.Identifier.Text));
		}

		public static Microsoft.CodeAnalysis.SyntaxList<Sharp.TypeParameterConstraintClauseSyntax> ConstraintList(TypeParameterConstraintListSyntax? node)
		{
			if (node is null)
			{
				return default;
			}

			return SyntaxFactory.List(node.Clauses
				.Select(x => SyntaxFactory.TypeParameterConstraintClause(
					SyntaxFactory.IdentifierName(x.Identifier.Text),
					SyntaxFactory.SeparatedList(x.Constraints
						.Select<TypeParameterConstraintSyntax, Sharp.TypeParameterConstraintSyntax>(x => x switch
						{
							ClassConstraintSyntax c => SyntaxFactory.ClassOrStructConstraint(CSyntaxKind.ClassConstraint),
							StructConstraintSyntax s => SyntaxFactory.ClassOrStructConstraint(CSyntaxKind.StructConstraint),
							TypeConstraintSyntax t => SyntaxFactory.TypeConstraint(Expressions.Type(t.Type)),
							_ => throw new UnreachableException()
						})
						// class and struct constraints must be defined first in C#
						.OrderBy(x => x is Sharp.ClassOrStructConstraintSyntax ? 0 : 1)
					)
				))
			);
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

			if (!HasFlag(flags, ModifierFlags.Mut) && node is FieldDeclarationSyntax)
			{
				targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.ReadOnlyKeyword));
			}

			if(!isStatic && ShouldAddStatic(node))
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

		private static Microsoft.CodeAnalysis.SyntaxList<Sharp.MemberDeclarationSyntax> Members(Syntax.Abstractions.SyntaxList<MemberDeclarationSyntax> members)
		{
			TypeFlags typeFlags = default;
			return Members(members, ref typeFlags);
		}

		private static Microsoft.CodeAnalysis.SyntaxList<Sharp.MemberDeclarationSyntax> Members(Syntax.Abstractions.SyntaxList<MemberDeclarationSyntax> members, ref TypeFlags typeFlags)
		{
			if(members.IsDefaultOrEmpty)
			{
				return default;
			}

			List<Sharp.MemberDeclarationSyntax> list = new(members.Count);

			foreach (MemberDeclarationSyntax member in members)
			{
				list.Add(Member(member, ref typeFlags));
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

				members = members.AddRange(Interop.DisposablePattern(isOpen));
			}

			if (HasFlag(typeFlags, TypeFlags.Destructor))
			{
				members = members.Add(Interop.IDisposableDestructor(SyntaxFactory.Identifier(node.Identifier.Text)));
			}
		}

		private static Sharp.AccessorListSyntax AccessorList(AccessorListSyntax node, bool isInit)
		{
			return SyntaxFactory.AccessorList(SyntaxFactory.List(node.Accessors.Select(x => Accessor(x, x.Kind switch
			{
				SyntaxKind.GetAccessorDeclaration => CSyntaxKind.GetKeyword,
				SyntaxKind.SetAccessorDeclaration => isInit
					? CSyntaxKind.InitKeyword
					: CSyntaxKind.SetKeyword,
				_ => throw new UnreachableException()
			}))));
		}

		private static Sharp.AccessorDeclarationSyntax Accessor(AccessorDeclarationSyntax node, CSyntaxKind kind)
		{
			CSyntaxKind decl = Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetAccessorDeclarationKind(kind);

			CSyntaxTokenList modifiers = GetModifiers(node, node.Modifiers, out _);

			if (node.Block is not null)
			{
				return SyntaxFactory.AccessorDeclaration(decl,
					Attributes(node.Attributes),
					modifiers,
					SyntaxFactory.Token(kind),
					Statements.Block(node.Block),
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
}
