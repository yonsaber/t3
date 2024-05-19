using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace examples.user.wake.livecoding
{
    [Guid("7a5c2f3b-c27a-47d1-8fc8-a9d7b508b9ce")]
    public class LiveCodingVideoComposition : Instance<LiveCodingVideoComposition>
    {
        [Output(Guid = "f1c21858-606c-4e60-af15-6e0ef9979fda")]
        public readonly Slot<Texture2D> ColorBuffer = new Slot<Texture2D>();


    }
}
