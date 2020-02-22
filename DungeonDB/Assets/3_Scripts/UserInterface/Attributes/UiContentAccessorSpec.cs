using System;
using Content;

namespace Content
{
	public class UiContentAccessorSpec : Attribute
	{
		#region Constructors

		public UiContentAccessorSpec(ContentType _contentType, UiControlContentBinding _binding, bool _displayContent = true)
		{
			contentType = _contentType;
			binding = _binding;
			displayContent = _displayContent;
		}

		#endregion
		#region Fields

		public ContentType contentType = ContentType.Unknown;
		public UiControlContentBinding binding = UiControlContentBinding.LoadFromDatabase;
		public UiControlBindingBehaviour bindingBehaviour = UiControlBindingBehaviour.HideIfNoBinding;
		public bool displayContent = true;

		#endregion
	}
}
