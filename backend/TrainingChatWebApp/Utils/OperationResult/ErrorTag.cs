namespace TrainingChatWebApp.Utils.OperationResult;

public struct ErrorTag { }

public struct ErrorTag<TError>
{
	internal readonly TError Error;

	internal ErrorTag(TError error)
	{
		Error = error;
	}
}
