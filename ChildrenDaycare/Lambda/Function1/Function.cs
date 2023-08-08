using Amazon.Lambda.Core;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace mylambdas.Function1
{
    public class Function
    {
        public string FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
        {
            var role = input.ContainsKey("role") ? input["role"] : string.Empty;

            switch (role)
            {
                case "Admin":
                    return "Index-Slot";
                case "Takecare Giver":
                    return "TakecareGiverDisplay-Slot";
                case "Public":
                    return "PublicDisplay-Slot";
                default:
                    return "Home";
            }
        }
    }
}
