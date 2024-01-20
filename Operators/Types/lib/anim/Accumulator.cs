using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_90b2c6d2_e9a6_4910_b42d_94202f07be27
{
    public class Accumulator : Instance<Accumulator>
    {
        [Output(Guid = "A999F93C-E51A-4325-BAE2-BFAB830B868D", DirtyFlagTrigger = DirtyFlagTrigger.Animated)]
        public readonly Slot<float> Result = new();

        public Accumulator()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            var running = Running.GetValue(context);
            
            
            var startValue = StartValue.GetValue(context);
            if (ResetTrigger.GetValue(context))
            {
                Result.Value = startValue;
                _v = startValue;
            }

            var increment = Increment.GetValue(context);

            var t = context.Playback.SecondsFromBars(context.LocalFxTime);
            var dt = t - _lastUpdateTime;
            _lastUpdateTime = t;

            if (running)
            {
                _v += increment * dt;
            }

            
            var modulo = Modulo.GetValue(context);
            Result.Value = modulo > 0 ? (float)(_v % modulo): (float)_v;


        }

        private double _lastUpdateTime;
        private double _v;
        
        [Input(Guid = "7CAF37EC-ED34-4711-B02C-E136D070FFF7")]
        public readonly InputSlot<bool> Running = new();


        [Input(Guid = "052f9cd1-4b3e-483f-b628-c356f58ff87e")]
        public readonly InputSlot<float> Increment = new();

        [Input(Guid = "26f05385-3c48-499a-bc30-69bad0a2218c")]
        public readonly InputSlot<float> StartValue = new();


        [Input(Guid = "9F1D76D2-C3DD-4035-A37C-300EB011331B")]
        public readonly InputSlot<bool> ResetTrigger = new();
        
        [Input(Guid = "4D90CD4B-8E11-4B86-A668-26810AF029B3")]
        public readonly InputSlot<float> Modulo = new();



    }
}