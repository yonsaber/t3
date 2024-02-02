using System.Runtime.InteropServices;
using T3.Core.DataTypes.Vector;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace lib.img.adjust
{
	[Guid("da93f7d1-ef91-4b4a-9708-2d9b1baa4c14")]
    public class RemapColor : Instance<RemapColor>
    {
        [Output(Guid = "16e37306-05e1-4de6-babd-80a8d1472a2f")]
        public readonly Slot<SharpDX.Direct3D11.Texture2D> TextureOutput = new();

        [Input(Guid = "876f6f64-7cb4-4060-8571-e0b78b437d41")]
        public readonly InputSlot<SharpDX.Direct3D11.Texture2D> Image = new();

        [Input(Guid = "cb52ff49-17de-4e36-b918-5de6973a234a")]
        public readonly InputSlot<Int2> Resolution = new();

        [Input(Guid = "c45d487b-3221-44c7-bf9e-b982a65280f6")]
        public readonly InputSlot<T3.Core.DataTypes.Gradient> Gradient = new();

        [Input(Guid = "7777f86d-dbf7-44d4-9da4-99a819038095")]
        public readonly InputSlot<bool> DontColorAlpha = new();

        [Input(Guid = "e3363c0e-819a-45e2-8202-439bcce64d69",MappedType = typeof(Modes))]
        public readonly InputSlot<int> Mode = new();


        private enum Modes
        {
            UseGrayScale,
            IndividualChannels,
        }
    }
}
