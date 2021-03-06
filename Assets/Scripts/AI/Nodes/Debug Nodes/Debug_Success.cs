using BehaviorTree;

public class Debug_Success : Node
{
    private RootNode _root;
    private bool _rootInitialized = false;
    public override (NodeState, Action) Evaluate()
    {
        if (!_rootInitialized)
            _root = GetRootNode();

        return (NodeState.SUCCESS, Action.None);
    }
    private RootNode GetRootNode()
    {
        Node currentNode = this;

        while (currentNode.parent != null)
            currentNode = currentNode.parent;

        return (RootNode)currentNode;
    }
}

