using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace user.pixtur.research
{
	[Guid("0fa509a8-086a-4b60-ab9a-ee979643a29e")]
    public class LineTransitionExperiment1 : Instance<LineTransitionExperiment1>
    {

        [Output(Guid = "bc0a0bd0-dd93-4d49-8823-eff86dbae5d1")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> Output2 = new();

    }
}
