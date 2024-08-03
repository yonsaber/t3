using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace examples.user.newemka980.Examples
{
    [Guid("e92bdb18-4a42-4a92-aacc-45765e5862bf")]
    public class SpringVec3Example : Instance<SpringVec3Example>
    {
        [Output(Guid = "15daaf5a-f3d9-4746-bc8a-f9cebb490f9c")]
        public readonly Slot<Texture2D> ColorBuffer = new Slot<Texture2D>();


    }
}
