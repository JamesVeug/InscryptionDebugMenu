namespace DebugMenu.Scripts.Acts;

public abstract class BaseMapSequence
{
	public virtual void OnGUI()
	{
		
	}
	
	public abstract void ToggleSkipNextNode();
	public abstract void ToggleAllNodes();
}