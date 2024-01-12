using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using T3.Core.DataTypes;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Utils;

namespace lib.img.fx
{
	[Guid("d392d4af-4c78-4f4a-bc3f-4c54c8c73538")]
    public class Glow : Instance<Glow>
    {
        [Output(Guid = "2ce1453b-432b-4d12-8fb7-d883e3d0c136")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> ImgOutput = new();
        
        [Output(Guid = "78523193-3df8-4189-88c0-46091d53892e")]
        public readonly Slot<Command> Output = new();



        [Input(Guid = "f6bdd487-c16e-4fb0-bfba-b3801f121314")]
        public readonly InputSlot<Texture2D> Texture = new();

        [Input(Guid = "57968725-0a45-44f9-a9a2-f74c10b728e8")]
        public readonly InputSlot<float> BlurRadius = new();

        [Input(Guid = "353ac2ee-aed3-4614-adf5-e1328768fd0b")]
        public readonly InputSlot<float> Samples = new();

        [Input(Guid = "4927a3fc-87ff-44e7-88c0-499e3efcca55")]
        public readonly InputSlot<float> GlowAmount = new();

        [Input(Guid = "4c9b9135-f27b-414e-bed7-f9e5640dc526")]
        public readonly InputSlot<float> Offset = new();

        [Input(Guid = "7f6dba80-bf4e-4e55-bd9a-ac1e2a077898")]
        public readonly InputSlot<System.Numerics.Vector4> Color = new();

        [Input(Guid = "03f2bd5c-b599-47f5-b697-9f881639a598")]
        public readonly InputSlot<float> AmplifyFineBlur = new();

        [Input(Guid = "53fe4db2-128c-43e3-8c58-8f01694d13ac", MappedType = typeof(SharedEnums.BlendModes))]
        public readonly InputSlot<int> BlendMode = new();

    }
}
