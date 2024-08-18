namespace FluffyUnderware.Curvy.Generator
{
	public interface IOnRequestProcessing
	{
		CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests);
	}
}
