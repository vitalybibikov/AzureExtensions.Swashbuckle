using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AzureFunctions.Extensions.Swashbuckle
{
    internal sealed class EmptyCompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
    {
        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
        }
    }
}
