using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaliCareApp.Models
{
    public class GlucoseValue : ValueObject
    {
        public float Value { get; private set; }
        public string Unit { get; private set; }
        private GlucoseValue()
        {
            
        }
        public GlucoseValue(float value, string unit)
        {
            Value = value;
            Unit = unit;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
            yield return Unit;
        }
    }
}
