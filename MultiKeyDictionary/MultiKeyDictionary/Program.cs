using System;

namespace MultiKeyDictionary
{
    class Program
    {
        static void Main(string[] args)
        {
            var dict = new DoubleKeyDictionary<TestType, int, int>();

            var testKey1 = new TestType
            {
                TestGuid = new Guid("3D5C91E2-6163-4396-8339-B503294C0B07"),
                TestString = "Test1"
            };

            var testKey2 = new TestType
            {
                TestGuid = new Guid("B19394AF-1A2C-4718-963D-09491E2CC275"),
                TestString = "Test2"
            };

            var testKey3 = new TestType
            {
                TestGuid = new Guid("A59EB319-B8A1-4FCF-BDB6-39C14E9BCD21"),
                TestString = "Test3"
            };

            dict.Add(testKey1, 1, 1);
            dict.Add(testKey2, 1, 2);
            dict.Add(testKey3, 1, 3);
            dict.Add(testKey1, 2, 4);
            dict.Add(testKey1, 3, 5);
            dict.Add(testKey2, 2, 7);
            dict.Add(testKey2, 3, 8);
            dict.Add(testKey3, 2, 10);
            dict.Add(testKey3, 3, 11);

            var byLeftKey1 = dict.GetValuesByLeftKey(testKey1); //1,4,5
            var byLeftKey2 = dict.GetValuesByLeftKey(testKey2); //2,7,8
            var byLeftKey3 = dict.GetValuesByLeftKey(testKey3); //3,10,11


            var byRightKey1 = dict.GetValuesByRightKey(1); //1,2,3
            var byRightKey2 = dict.GetValuesByRightKey(2); //4,7,10
            var byRightKey3 = dict.GetValuesByRightKey(3); //5,8,11
        }

        private class TestType
        {
            public Guid TestGuid { get; set; }

            public string TestString { get; set; }

            protected bool Equals(TestType other)
            {
                return TestGuid.Equals(other.TestGuid) && string.Equals(TestString, other.TestString);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestType) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (TestGuid.GetHashCode() * 397) ^ (TestString != null ? TestString.GetHashCode() : 0);
                }
            }
        }
    }
}