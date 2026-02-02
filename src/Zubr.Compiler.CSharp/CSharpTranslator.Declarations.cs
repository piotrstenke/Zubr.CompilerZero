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

		public static Sharp.MemberDeclarationSyntax Member(MemberDeclarationSyntax node)
		{
			return node switch
			{
				ClassDeclarationSyntax c => Class(c),
				StructDeclarationSyntax s => Struct(s),
				TraitDeclarationSyntax t => Interface(t),
				ImplementationDeclarationSyntax i => Extension(i),
				BaseFunctionDeclarationSyntax f => Method(f),
				PropertyDeclarationSyntax p => Property(p),
				SimpleEnumDeclarationSyntax e => Enum(e),
				_ => throw new UnreachableException()
			};
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

			if (flags.HasFlag(ModifierFlags.Limit))
			{
				attributes = attributes.Add(Interop.InternalInheritAttribute());
			}

			if (flags.HasFlag(ModifierFlags.Data))
			{
				return SyntaxFactory.RecordDeclaration(
					attributes,
					modifiers,
					default,
					SyntaxFactory.Identifier(node.Identifier.Text),
					typeParameterList,
					parameters,
					BaseTypeList(node.BaseTypeList),
					constraints,
					SyntaxFactory.List(node.Members.Select(Member))
				);
			}

			return SyntaxFactory.ClassDeclaration(
				attributes,
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				parameters,
				BaseTypeList(node.BaseTypeList),
				constraints,
				SyntaxFactory.List(node.Members.Select(Member))
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
				SyntaxFactory.List(node.Members.Select(Member))
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

			if (flags.HasFlag(ModifierFlags.Data))
			{
				return SyntaxFactory.RecordDeclaration(
					Attributes(node.Attributes),
					modifiers,
					SyntaxFactory.Token(CSyntaxKind.StructKeyword),
					SyntaxFactory.Identifier(node.Identifier.Text),
					typeParameterList,
					parameters,
					BaseTypeList(node.BaseTypeList),
					constraints,
					SyntaxFactory.List(node.Members.Select(Member))
				);
			}

			return SyntaxFactory.StructDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				BaseTypeList(node.BaseTypeList),
				constraints,
				SyntaxFactory.List(node.Members.Select(Member))
			);
		}

		public static Sharp.InterfaceDeclarationSyntax Interface(TraitDeclarationSyntax node)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			Sharp.TypeParameterListSyntax? typeParameterList = TypeParameterList(node.TypeParameterList);
			var constraints = ConstraintList(node.ConstraintList);

			Sharp.ParameterListSyntax? parameters = node.ParameterList is null
				? null
				: ParameterList(node.ParameterList);

			return SyntaxFactory.InterfaceDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Identifier(node.Identifier.Text),
				typeParameterList,
				BaseTypeList(node.BaseTypeList),
				constraints,
				SyntaxFactory.List(node.Members.Select(Member))
			);
		}

		public static Sharp.EnumDeclarationSyntax Enum(SimpleEnumDeclarationSyntax node)
		{
			// TODO: Implement enum struct and enum class.

			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			if(flags.HasFlag(ModifierFlags.Flag))
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

			if (flags.HasFlag(ModifierFlags.Base))
			{
				if (node.ExpressionBody is null)
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.AbstractKeyword));
				}
				else
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.VirtualKeyword));
					attributes = attributes.Add(Interop.MustOverrideAttribute());
				}
			}

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
					null
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
					Initializer(node.Initializer)
				);
			}

			AccessorDeclarationSyntax? getAccessorSource = GetAccessor(node, TokenKind.GetKeyword);
			AccessorDeclarationSyntax? setAccessorSource = GetAccessor(node, TokenKind.SetKeyword);

			// Zubr does not support set-only properties.
			Sharp.AccessorDeclarationSyntax getAccessor = getAccessorSource is null
				? SyntaxFactory.AccessorDeclaration(CSyntaxKind.GetAccessorDeclaration)
				: Accessor(getAccessorSource, CSyntaxKind.GetAccessorDeclaration);

			Sharp.AccessorDeclarationSyntax? setAccessor = null;

			if (setAccessorSource is null)
			{
				if (flags.HasFlag(ModifierFlags.Mut))
				{
					setAccessor = SyntaxFactory.AccessorDeclaration(CSyntaxKind.SetAccessorDeclaration);
				}
				else if (flags.HasFlag(ModifierFlags.Init))
				{
					setAccessor = SyntaxFactory.AccessorDeclaration(CSyntaxKind.InitAccessorDeclaration);
				}
			}
			else
			{
				if (flags.HasFlag(ModifierFlags.Init))
				{
					setAccessor = Accessor(setAccessorSource, CSyntaxKind.InitAccessorDeclaration);
				}
				else
				{
					setAccessor = Accessor(setAccessorSource, CSyntaxKind.SetAccessorDeclaration);
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
				Initializer(node.Initializer)
			);

			static AccessorDeclarationSyntax? GetAccessor(PropertyDeclarationSyntax node, TokenKind keyword)
			{
				return node.AccessorList!.Accessors.FirstOrDefault(x => x.Keyword.IsKind(keyword));
			}

			static Sharp.AccessorListSyntax GenerateAccessorList(ModifierFlags flags)
			{
				Sharp.AccessorDeclarationSyntax getAccessor = SyntaxFactory.AccessorDeclaration(CSyntaxKind.GetAccessorDeclaration);
				Sharp.AccessorListSyntax accessorList;

				if (flags.HasFlag(ModifierFlags.Mut))
				{
					accessorList = SyntaxFactory.AccessorList(SyntaxFactory.List([
							getAccessor,
						SyntaxFactory.AccessorDeclaration(CSyntaxKind.SetAccessorDeclaration)
					]));
				}
				else if (flags.HasFlag(ModifierFlags.Init))
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

		public static Sharp.BaseMethodDeclarationSyntax Method(BaseFunctionDeclarationSyntax node)
		{
			return node switch
			{
				FunctionDeclarationSyntax f => Method(f),
				ConstructorDeclarationSyntax c => Constructor(c),
				DestructorDeclarationSyntax d => Destructor(d),
				_ => throw new UnreachableException()
			};
		}

		public static Sharp.MethodDeclarationSyntax Method(FunctionDeclarationSyntax node)
		{
			var attributes = Attributes(node.Attributes);
			CSyntaxTokenList modifiers = GetModifiers(node, out ModifierFlags flags);

			if (flags.HasFlag(ModifierFlags.Base))
			{
				if(node.ExpressionBody is null && node.Body is null)
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.AbstractKeyword));
				}
				else
				{
					modifiers = modifiers.Add(SyntaxFactory.Token(CSyntaxKind.VirtualKeyword));
					attributes = attributes.Add(Interop.MustOverrideAttribute());
				}
			}

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

		public static Sharp.DestructorDeclarationSyntax Destructor(DestructorDeclarationSyntax node)
		{
			// TODO: Implement the IDisposable interface for the parent type.

			CSyntaxTokenList modifiers = GetModifiers(node, out _);
			CSyntaxToken identifier = node.Parent is TypeDeclarationSyntax parentType
				? SyntaxFactory.Identifier(parentType.Identifier.Text)
				: default;

			Sharp.ParameterListSyntax parameters = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(node.ParameterList.Parameters.Select(Parameter)));

			return SyntaxFactory.DestructorDeclaration(
				Attributes(node.Attributes),
				modifiers,
				SyntaxFactory.Token(CSyntaxKind.TildeToken),
				identifier,
				parameters,
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
			bool isPrivate = false;

			for (int i = 0; i < modifiers.Count; i++)
			{
				TokenKind kind = modifiers[i].Kind;

				switch(kind)
				{
					case TokenKind.PrivKeyword:
						targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.PrivateKeyword));
						hasAccessModifiers = true;
						isPrivate = true;
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
				if (node is ClassDeclarationSyntax)
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.SealedKeyword));
				}
			}

			if(isPrivate)
			{
				// Private properties are translated as fields, so apply 'readonly' modifier if the property is not mutable.
				if(!flags.HasFlag(ModifierFlags.Mut) && node is PropertyDeclarationSyntax)
				{
					targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.ReadOnlyKeyword));
				}
			}

			if (node is FunctionDeclarationSyntax && node.Parent is CompilationUnitSyntax or ModuleDeclarationSyntax)
			{
				targetModifiers.Add(SyntaxFactory.Token(CSyntaxKind.StaticKeyword));
			}

			return SyntaxFactory.TokenList(targetModifiers.ToArray());
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

		private static Sharp.AccessorDeclarationSyntax Accessor(AccessorDeclarationSyntax node, CSyntaxKind kind)
		{
			CSyntaxTokenList modifiers = GetModifiers(node, node.Modifiers, out _);

			if (node.Block is not null)
			{
				return SyntaxFactory.AccessorDeclaration(kind,
					Attributes(node.Attributes),
					modifiers,
					Statements.Block(node.Block)
				);
			}

			if (node.ExpressionBody is not null)
			{
				return SyntaxFactory.AccessorDeclaration(kind,
					Attributes(node.Attributes),
					modifiers,
					Statements.ExpressionBody(node.ExpressionBody)
				);
			}

			return SyntaxFactory.AccessorDeclaration(kind,
				Attributes(node.Attributes),
				modifiers,
				null,
				null
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

		[return: NotNullIfNotNull(nameof(node))]
		private static Sharp.EqualsValueClauseSyntax? Initializer(EqualsValueClauseSyntax? node)
		{
			if (node is null)
			{
				return null;
			}

			return SyntaxFactory.EqualsValueClause(Expressions.Expression(node.Value));
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

		Init = 32
	}
}
