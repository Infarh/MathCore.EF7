using MathCore.EF7.Enities.Base;

namespace Domain
{
    public class Student :Entity
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
