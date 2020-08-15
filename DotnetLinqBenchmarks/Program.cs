using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotnetLinqBenchmarks
{
    public readonly struct Customer
    {
        public int Age { get; }
        public string Name { get; }

        public bool IsMinor => Age < 18;

        public Customer(int age, string name) => (Age, Name) = (age, name);
    }

    public class Program
    {
        public static void Main(string[] _)
        {
            BenchmarkRunner.Run<IterationExperiments>();
            BenchmarkRunner.Run<FilterExperiments>();
            BenchmarkRunner.Run<TransformationExperiments>();
        }
    }

    [RPlotExporter]
    [SimpleJob(id: "Experiment")]
    public class Experiments
    {
        protected Customer[] customers;

        [Params(100_000, 200_000, 300_000, 400_000, 500_000, 600_000, 700_000, 800_000, 900_000, 1_000_000, 2_000_000, 3_000_000)]
        public int size;

        [IterationSetup]
        public void Setup()
        {
            customers = new Customer[size];
            for (int i = 0; i < size; i++)
            {
                customers[i] = new Customer(i % 151, $"customer {i}"); // limit age to [0, 150]
            }
        }
    }

    public class IterationExperiments : Experiments
    {
        [Benchmark(Description = "for")]
        public int CountMinorCustomers_ForLoop()
        {
            int count = 0;
            for (int i = 0; i < customers.Length; i++)
            {
                if (customers[i].IsMinor)
                {
                    count++;
                }
            }
            return count;
        }

        [Benchmark(Description = "foreach")]
        public int CountMinorCustomers_ForEachLoop()
        {
            int count = 0;
            foreach (var c in customers)
            {
                if (c.IsMinor)
                {
                    count++;
                }
            }
            return count;
        }

        [Benchmark(Description = "LINQ count")]
        public int CountMinorCustomers_LINQ_SelectCount() => customers.Where(c => c.IsMinor).Count();

        [Benchmark(Description = "LINQ count pred")]
        public int CountMinorCustomers_LINQ_CountPredicate() => customers.Count(c => c.IsMinor);
    }

    public class FilterExperiments : Experiments
    {
        [Benchmark(Baseline = true, Description = "for")]
        public List<Customer> WithoutMinorCustomers_ForLoop()
        {
            var results = new List<Customer>();
            for (int i = 0; i < customers.Length; i++)
            {
                var c = customers[i];
                if (!c.IsMinor)
                {
                    results.Add(c);
                }
            }
            return results;
        }

        [Benchmark(Description = "foreach")]
        public List<Customer> WithoutMinorCustomers_ForEachLoop()
        {
            var results = new List<Customer>();
            foreach (var c in customers)
            {
                if (!c.IsMinor)
                {
                    results.Add(c);
                }
            }
            return results;
        }

        [Benchmark(Description = "LINQ where")]
        public List<Customer> WithoutMinorCustomers_LINQ() => customers.Where(c => !c.IsMinor).ToList();
    }

    public class TransformationExperiments : Experiments
    {
        public Customer ToAgeInDays(Customer c) => new Customer(c.Age * 365, c.Name);

        [Benchmark(Baseline = true, Description = "for")]
        public List<Customer> CustomersToAgeInDays_For()
        {
            var customersWithAgeInDays = new List<Customer>(customers.Length);
            for (int i = 0; i < customers.Length; i++)
            {
                customersWithAgeInDays.Add(ToAgeInDays(customers[i]));
            }
            return customersWithAgeInDays;
        }

        [Benchmark(Description = "foreach")]
        public List<Customer> CustomersToAgeInDays_ForEach()
        {
            var customersWithAgeInDays = new List<Customer>(customers.Length);
            foreach (var c in customers)
            {
                customersWithAgeInDays.Add(ToAgeInDays(c));
            }
            return customersWithAgeInDays;
        }

        [Benchmark(Description = "LINQ select")]
        public List<Customer> CustomersToAgeInDays_LINQ() => customers.Select(ToAgeInDays).ToList();
    }
}
