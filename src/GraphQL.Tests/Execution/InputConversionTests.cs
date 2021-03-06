﻿using System;
using System.Collections.Generic;
using System.Linq;
using Should;

namespace GraphQL.Tests.Execution
{
    public class InputConversionTests
    {
        public class MyInput
        {
            public int A { get; set; }
            public string B { get; set; }
            public List<string> C { get; set; }
            public int? D { get; set; }
            public Guid E { get; set; }
            public List<int?> F { get; set; }
            public List<List<int?>> G { get; set; }
        }

        public class EnumInput
        {
            public Numbers A { get; set; }
            public Numbers2? B { get; set; }
        }

        public enum Numbers
        {
            One,
            Two,
            Three
        }

        public enum Numbers2 : long
        {
            One,
            Two,
            Three
        }

        public class Parent2
        {
            public Parent Input { get; set; }
        }

        public class Parent
        {
            public string A { get; set; }
            public List<Child> B { get; set; }
            public string C { get; set; }
        }

        public class Child
        {
            public string A { get; set; }
        }

        [Fact]
        public void converts_small_numbers_to_int()
        {
            var json = @"{'a': 1}";
            var inputs = json.ToInputs();
            inputs["a"].ShouldEqual(1);
            inputs["a"].GetType().ShouldEqual(typeof(int));
        }

        [Fact]
        public void converts_large_numbers_to_long()
        {
            var json = @"{'a': 1000000000000000001}";
            var inputs = json.ToInputs();
            inputs["a"].ShouldEqual(1000000000000000001);
            inputs["a"].GetType().ShouldEqual(typeof(long));
        }

        [Fact]
        public void can_convert_json_to_input_object_and_specific_object()
        {
            var json = @"{'a': 1, 'b': '2'}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();
            inputs["a"].ShouldEqual((int)1);
            inputs["b"].ShouldEqual("2");

            var myInput = inputs.ToObject<MyInput>();

            myInput.ShouldNotBeNull();
            myInput.A.ShouldEqual(1);
            myInput.B.ShouldEqual("2");
        }

        [Fact]
        public void can_convert_json_to_array()
        {
            var json = @"{'a': 1, 'b': '2', 'c': ['foo']}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();
            inputs["a"].ShouldEqual((int)1);
            inputs["b"].ShouldEqual("2");
            inputs["c"].ShouldBeType<List<object>>();

            var myInput = inputs.ToObject<MyInput>();

            myInput.ShouldNotBeNull();
            myInput.A.ShouldEqual(1);
            myInput.B.ShouldEqual("2");
            myInput.C.ShouldNotBeNull();
            myInput.C.Count.ShouldEqual(1);
            myInput.C.First().ShouldEqual("foo");
        }

        [Fact]
        public void can_convert_json_to_nullable_array()
        {
            var json = @"{'a': 1, 'b': '2', 'c': ['foo'], 'f': [1,null]}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();
            inputs["f"].ShouldBeType<List<object>>();

            var myInput = inputs.ToObject<MyInput>();
            myInput.ShouldNotBeNull();
            myInput.F.ShouldNotBeNull();
            myInput.F.Count.ShouldEqual(2);
            myInput.F[0].ShouldEqual(1);
            myInput.F[1].ShouldEqual(null);
        }

        [Fact]
        public void can_convert_json_to_nested_nullable_array()
        {
            var json = @"{'a': 1, 'b': '2', 'c': ['foo'], 'g': [[1,null], [null, 1]]}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();
            inputs["g"].ShouldBeType<List<object>>();

            var myInput = inputs.ToObject<MyInput>();
            myInput.ShouldNotBeNull();
            myInput.G.ShouldNotBeNull();
            myInput.G.Count.ShouldEqual(2);

            myInput.G[0].Count.ShouldEqual(2);
            myInput.G[0][0].ShouldEqual(1);
            myInput.G[0][1].ShouldBeNull();

            myInput.G[1].Count.ShouldEqual(2);
            myInput.G[1][0].ShouldBeNull();
            myInput.G[1][1].ShouldEqual(1);
        }

        [Fact]
        public void can_convert_json_to_input_object_with_nullable_int()
        {
            var json = @"{'a': 1, 'b': '2', 'd': '5'}";
            var inputs = json.ToInputs();
            inputs.ShouldNotBeNull();
            var myInput = inputs.ToObject<MyInput>();
            myInput.ShouldNotBeNull();
            myInput.D.ShouldEqual(5);
        }

        [Fact]
        public void can_convert_json_to_input_object_with_guid()
        {
            var json = @"{'a': 1, 'b': '2', 'e': '920a1b6d-f75a-4594-8567-e2c457b29cc0'}";
            var inputs = json.ToInputs();
            inputs.ShouldNotBeNull();
            var myInput = inputs.ToObject<MyInput>();
            myInput.ShouldNotBeNull();
            myInput.E.ShouldEqual(new Guid("920a1b6d-f75a-4594-8567-e2c457b29cc0"));
        }

        [Fact]
        public void can_convert_json_to_input_object_with_enum_string()
        {
            var json = @"{'a': 'three'}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();

            var myInput = inputs.ToObject<EnumInput>();

            myInput.ShouldNotBeNull();
            myInput.A.ShouldEqual(Numbers.Three);
            myInput.B.ShouldBeNull();
        }

        [Fact]
        public void can_convert_json_to_input_object_with_enum_string_exact()
        {
            var json = @"{'a': 'Two'}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();

            var myInput = inputs.ToObject<EnumInput>();

            myInput.ShouldNotBeNull();
            myInput.A.ShouldEqual(Numbers.Two);
        }

        [Fact]
        public void can_convert_json_to_input_object_with_enum_number()
        {
            var json = @"{'a': '2'}";

            var inputs = json.ToInputs();

            inputs.ShouldNotBeNull();

            var myInput = inputs.ToObject<EnumInput>();

            myInput.ShouldNotBeNull();
            myInput.A.ShouldEqual(Numbers.Three);
            myInput.B.ShouldBeNull();
        }

        [Fact]
        public void can_convert_json_to_input_object_with_enum_long_number()
        {
            var json = @"{'a': 2, 'b': 2}";

            var inputs = json.ToInputs();

            var myInput = inputs.ToObject<EnumInput>();

            myInput.ShouldNotBeNull();
            myInput.B.Value.ShouldEqual(Numbers2.Three);
        }

        [Fact]
        public void can_convert_json_to_input_object_with_child_object_list()
        {
            var json = @"{'a': 'foo', 'b':[{'a':'bar'}], 'c': 'baz'}";

            var inputs = json.ToInputs();

            var myInput = inputs.ToObject<Parent>();

            myInput.ShouldNotBeNull();
            myInput.B.Count.ShouldEqual(1);
            myInput.B[0].A.ShouldEqual("bar");
        }

        [Fact]
        public void can_convert_json_to_input_object_with_child_object()
        {
            var json = @"{ 'input': {'a': 'foo', 'b':[{'a':'bar'}], 'c': 'baz'}}";

            var inputs = json.ToInputs();

            var myInput = inputs.ToObject<Parent2>();

            myInput.ShouldNotBeNull();
            myInput.Input.B.Count.ShouldEqual(1);
            myInput.Input.B[0].A.ShouldEqual("bar");
        }
    }
}
