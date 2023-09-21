using System;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_dc3d1571_ad9f_46aa_bed9_df2f4e1c7040
{
    public class ParticleSimulation : Instance<ParticleSimulation>
    {

        [Output(Guid = "fd2f84af-0925-418e-b3fa-edec6fa19df3")]
        public readonly Slot<T3.Core.DataTypes.BufferWithViews> OutBuffer = new Slot<T3.Core.DataTypes.BufferWithViews>();

        [Input(Guid = "088f9a81-7170-4f9d-bbfa-f08b0bf32317")]
        public readonly InputSlot<T3.Core.DataTypes.BufferWithViews> EmitPoints = new InputSlot<T3.Core.DataTypes.BufferWithViews>();

        [Input(Guid = "5525b00a-eea5-46ed-b4b4-cbcadcee3820")]
        public readonly InputSlot<bool> Emit = new InputSlot<bool>();

        [Input(Guid = "267b6cae-2c3d-4874-9532-ca3da138fde6")]
        public readonly InputSlot<bool> Reset = new InputSlot<bool>();

        [Input(Guid = "18903940-ff20-4b64-a4f0-6078977edd7a")]
        public readonly InputSlot<int> MaxParticleCount = new InputSlot<int>();

        [Input(Guid = "a03ffef9-11e3-41f9-9f13-71f107b484df")]
        public readonly InputSlot<float> AgingRate = new InputSlot<float>();

        [Input(Guid = "0f84199d-76f0-4155-b5b0-f6d05260423a")]
        public readonly InputSlot<float> MaxAge = new InputSlot<float>();

        [Input(Guid = "013912ef-60e8-4d31-b804-fe2a47ac9830", MappedType = typeof(SetWModes))]
        public readonly InputSlot<int> SetWTo = new InputSlot<int>();

        [Input(Guid = "3e6ff5e3-56a8-4be0-a918-ef041828e95f")]
        public readonly InputSlot<float> Speed = new InputSlot<float>();

        [Input(Guid = "79f17c7d-7ffe-43df-af17-36e97ab3813f")]
        public readonly InputSlot<float> Drag = new InputSlot<float>();

        [Input(Guid = "998e0875-ddad-49f0-b9cc-1ae5017a4bb6")]
        public readonly InputSlot<bool> SetInitialVelocity = new InputSlot<bool>();

        [Input(Guid = "22670413-93a6-4743-8ef3-962a975410de")]
        public readonly InputSlot<float> InitialVelocity = new InputSlot<float>();

        [Input(Guid = "7bf0b7de-359f-4561-8725-5c3c3407e91b")]
        public readonly MultiInputSlot<T3.Core.DataTypes.ParticleSystem> ParticleEffects = new MultiInputSlot<T3.Core.DataTypes.ParticleSystem>();
        
        private enum SetWModes {
            KeepOriginal,
            Age,
            Speed,
        }
    }
}

