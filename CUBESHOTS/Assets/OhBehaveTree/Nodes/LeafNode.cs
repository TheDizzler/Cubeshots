using System;
using System.Reflection;
using UnityEngine.Events;

namespace AtomosZ.OhBehave
{
	public class LeafNode : INode
	{
		public MethodInfo actionInfo;
		private OhBehaveActions ownerActions;


		public LeafNode(OhBehaveActions owner)
		{
			nodeType = NodeType.Leaf;
			ownerActions = owner;
		}



		public override INode Init()
		{
			nodeState = NodeState.Running;
			return this;
		}

		/// <summary>
		/// If nodeState is anything except Running, then return control to the parent.
		/// </summary>
		/// <returns></returns>
		public override NodeState Evaluate()
		{
			/* This is what the invokation would look like....*/
			var result = actionInfo.Invoke(ownerActions, new object[] { this });

			return nodeState;
		}


		public override INode Exit()
		{
			return parent.ChildFinished(nodeState);
		}
	}
}