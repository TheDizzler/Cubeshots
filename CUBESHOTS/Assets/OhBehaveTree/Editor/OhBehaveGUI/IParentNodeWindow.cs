namespace AtomosZ.OhBehave.EditorTools
{
	public interface IParentNodeWindow
	{
		void CreateChildConnection(NodeWindow newNode);
		string GetName();
	}
}