using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDemo
{
    public class VehicleEvent
    {
        public string id;
        public string vin;
        public string eventName;
        public string model;
        public double s1;
        public double s2;
        public double s3;
        public double s4;
        public double s5;
        public double s6;


        public static List<VehicleEvent> CreateEvent(int itemsToInsert)
        {
            var eventTypes = new string[] { "Harsh_break", "Airbag_deploy", "Check_engine_light" };
            string eventName = eventTypes[RandomNext(1, 3)];

            //Generate fake customer data.
            Bogus.Faker<VehicleEvent> _event = new Bogus.Faker<VehicleEvent>().Rules((faker, _event) =>
            {
                _event.id = Guid.NewGuid().ToString();
                _event.vin = faker.Vehicle.Vin();
                _event.eventName = eventName;
                _event.model = faker.Vehicle.Model();
                _event.s1 = 8442291.3;
                _event.s2 = 23959381.2;
                _event.s3 = 148;
                _event.s4 = 323;
                _event.s5 = 32395.9;
                _event.s6 = 8732;
            });

            return _event.Generate(itemsToInsert);
        }

        //Thread-safe random number generator
        private static Random _global = new Random();
        [ThreadStatic]
        private static Random _local;
        public static int RandomNext()
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next();
                _local = inst = new Random(seed);
            }
            return inst.Next();
        }
        public static int RandomNext(int minValue, int maxValue)
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global) seed = _global.Next(minValue, maxValue);
                _local = inst = new Random(seed);
            }
            return inst.Next(minValue, maxValue);
        }
    }
}
