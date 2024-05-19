using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace examples.lib.point
{
    [Guid("eb7bd521-c84b-45ad-88c6-a1ed79e64806")]
    public class SplinePointsExample : Instance<SplinePointsExample>
    {
        [Output(Guid = "e1e8ec79-d528-496e-a769-0d2c526b9cf0")]
        public readonly Slot<Texture2D> ColorBuffer = new Slot<Texture2D>();


    }
}
