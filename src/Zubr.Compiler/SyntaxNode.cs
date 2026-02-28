using System.Diagnostics;
using Zubr.Compiler.Syntax.Abstractions;
using Zubr.Compiler.Text;

namespace Zubr.Compiler;

[DebuggerDisplay("{ToString(),nq}")]
public abstract class SyntaxNode
{
	public SyntaxNode? Parent { get; private set; }

	public SyntaxTree SyntaxTree { get; }

	public int Position => Span.Start;

	public TextSpan Span { get; }

	public Location Location => SyntaxTree.GetLocation(Span);

	public abstract SyntaxKind Kind { get; }

	internal SyntaxNode(SyntaxTree tree, TextSpan span)
	{
		SyntaxTree = tree;
		Span = span;
	}

	public bool IsKind(SyntaxKind kind)
	{
		return Kind == kind;
	}

	public override string ToString()
	{
		return $"{Kind} at {Location}";
	}

	private protected void SetParent<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
	{
		if(list.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (TNode node in list)
		{
			node.Parent = this;
		}
	}

	private protected void SetParent<TNode>(SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
	{
		if (list.IsDefaultOrEmpty)
		{
			return;
		}

		foreach (TNode node in list)
		{
			node.Parent = this;
		}
	}

	private protected void SetParent(SyntaxNode node)
	{
		node.Parent = this;
	}

	private protected void SetParentIfNotNull(SyntaxNode? node)
	{
		node?.Parent = this;
	}
}
