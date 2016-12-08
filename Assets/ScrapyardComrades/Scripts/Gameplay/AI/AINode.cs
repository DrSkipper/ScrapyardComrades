using System.Collections.Generic;

public class AINode
{
    public AINode Parent;
    public AINode[] Children;

    public enum GraphAction
    {
        Stay,
        Continue,
        Split,
        Pop
    }

    public AINode(AINode parent)
    {
        this.Parent = parent;
    }

    public virtual AIOutput? runNode(AIInput input, out AINode nextNode)
    {
        nextNode = this;
        return new AIOutput();
    }
}

public class IdleNode : AINode
{
    public IdleNode(AINode parent) : base(parent)
    {
        this.Children = null;
    }
}

public class SplitterNode : AINode
{
    public int Index = 0;

    public SplitterNode(AINode parent, AINode first, AINode second = null) : base(parent)
    {
        this.Children = new AINode[second == null ? 1 : 2];
        this.Children[0] = first;
        if (second != null)
            this.Children[1] = second;
    }

    public SplitterNode(AINode parent, List<AINode> children) : base(parent)
    {
        this.Children = children.ToArray();
    }

    public AINode incrementNode()
    {
        if (this.Index == this.Children.Length)
        {
            this.Index = 0;
            if (this.Parent == null)
                return this.Children[0];
            return this.Parent;
        }

        ++this.Index;
        return this.Children[this.Index];
    }

    public override AIOutput? runNode(AIInput input, out AINode nextNode)
    {
        nextNode =  this.Children[this.Index];
        return null;
    }
}
