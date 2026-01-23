using Zubr.Compiler.Syntax.Abstractions;

namespace Zubr.Compiler;

public abstract class SyntaxNode
{
	public int Position { get; internal set; }

	public SyntaxNode? Parent { get; internal set; }

	public abstract SyntaxKind Kind { get; }

	public bool IsKind(SyntaxKind kind)
	{
		return Kind == kind;
	}

	private protected void SetParent<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
	{
		foreach (TNode node in list)
		{
			node.Parent = this;
		}
	}

	private protected void SetParent<TNode>(SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
	{
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
