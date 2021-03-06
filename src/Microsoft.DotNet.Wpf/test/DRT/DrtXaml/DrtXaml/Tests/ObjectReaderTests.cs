// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DrtXaml.Tests
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using DrtXaml.XamlTestFramework;
    using Test.Elements;
    using Test.Elements.Derived;
    using DRT;
    using System.Collections.Specialized;
    using System.Windows.Media;
    using System.Windows.Documents;

    [TestClass]
    class ObjectReaderTests : XamlTestSuite
    {
        public ObjectReaderTests()
            : base("ObjectReaderTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        public void DictionaryWithName()
        {
            var bar = new DerivedDictionary() { { "Hello", "World" } };
            TwoDictionaries foo = new TwoDictionaries() { One = bar, Two = bar };

            string generated = SaveToString(foo);
            string expected = 
@"<TwoDictionaries Two=""{x:Reference __ReferenceID0}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <TwoDictionaries.One>
    <DerivedDictionary x:Name=""__ReferenceID0"">
      <x:String x:Key=""Hello"">World</x:String>
    </DerivedDictionary>
  </TwoDictionaries.One>
</TwoDictionaries>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameReferenceConverter()
        {
            var foo = new Element();
            var bar = new NameReferencedHoldsTwoElements { One = foo, Two = foo };
            string generated = SaveToString(bar);
            string expected = 
@"<NameReferencedHoldsTwoElements Two=""__ReferenceID0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameReferencedHoldsTwoElements.One>
    <Element x:Name=""__ReferenceID0"" />
  </NameReferencedHoldsTwoElements.One>
</NameReferencedHoldsTwoElements>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SimpleNameConverter()
        {
            var nameElement = new NameElement();
            nameElement.Container = new HoldsOneElement { Element = nameElement };
            string generated = SaveToString(nameElement);
            string expected = 
@"<NameElement x:Name=""__ReferenceID0"" Container=""__ReferenceID0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SimpleNameConverterWithRuntimeName()
        {
            var nameElement = new NameElementWithRuntimeName() { Name = "Foo" };
            nameElement.Container = new HoldsOneElement { Element = nameElement };
            string generated = SaveToString(nameElement);
            string expected =
@"<NameElementWithRuntimeName Container=""Foo"" Name=""Foo"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameRequestedInInvisibleNameScope()
        {
            var foo = new Element();

            var bar = new NameElement();
            bar.Container = new HoldsOneElement { Element = foo };

            var ns = new NameScope { Content = foo };
            var ns2 = new NameScope { Content = bar };
            var arr = new object[] { ns, ns2 };

            var generated = SaveToString(arr);

            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope>
    <Element />
  </NameScope>
  <NameScope>
    <NameElement Container=""__ReferenceID0"" />
  </NameScope>
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameRequestedInInvisibleNameScope2()
        {
            var foo = new Element();

            var bar = new NameElement();
            bar.Container = new HoldsOneElement { Element = foo };

            var ns = new ObjectContainer { O = bar };
            var ns2 = new NameScope { Content = foo };
            var arr = new object[] { ns2, ns };

            var generated = SaveToString(arr);

            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope>
    <Element />
  </NameScope>
  <ObjectContainer>
    <ObjectContainer.O>
      <NameElement Container=""__ReferenceID0"" />
    </ObjectContainer.O>
  </ObjectContainer>
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameRequestedForObjectInRootNameScope()
        {
            var foo = new Element();

            var bar = new NameElement();
            bar.Container = new HoldsOneElement { Element = foo };

            var ns = new ObjectContainer { O = foo };
            var ns2 = new NameScope { Content = bar };
            var arr = new object[] { ns, ns2 };

            var generated = SaveToString(arr);

            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ObjectContainer>
    <ObjectContainer.O>
      <Element x:Name=""__ReferenceID0"" />
    </ObjectContainer.O>
  </ObjectContainer>
  <NameScope>
    <NameElement Container=""__ReferenceID0"" />
  </NameScope>
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MultipleGetNames()
        {
            var foo = new Element();

            var bar = new NameElement();
            bar.Container = new HoldsOneElement { Element = foo };

            var bar2 = new NameElement();
            bar2.Container = new HoldsOneElement { Element = foo };

            var arr = new object[] { bar, bar2, foo };

            var generated = SaveToString(arr);

            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameElement Container=""__ReferenceID0"" />
  <NameElement Container=""__ReferenceID0"" />
  <Element x:Name=""__ReferenceID0"" />
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MultipleGetNamesInDifferentNamescopes()
        {
            var foo = new Element();

            var bar = new NameElement();
            bar.Container = new HoldsOneElement { Element = foo };

            var bar2 = new NameElement();
            bar2.Container = new HoldsOneElement { Element = foo };

            var ns = new NameScope { Content = bar2 };

            var arr = new object[] { bar, ns, foo };

            var generated = SaveToString(arr);

            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameElement Container=""__ReferenceID0"" />
  <NameScope>
    <NameElement Container=""__ReferenceID0"" />
  </NameScope>
  <Element x:Name=""__ReferenceID0"" />
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ContentPropertyWithUselessTypeConverter()
        {
            var o = new UselessConverterContainer { Element = new Element() };
            string generated = SaveToString(o);
            string expected = 
@"<UselessConverterContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <Element />
</UselessConverterContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ReadWriteNonSelfInstantiatingDictionary()
        {
            var container = new ReadWriteDictionaryContainer() { Dictionary = new Hashtable() };
            container.Dictionary.Add("hello", "world");
            string generated = SaveToString(container);
            string expected =
@"<ReadWriteDictionaryContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ReadWriteDictionaryContainer.Dictionary>
    <x:String>world<x:Key><x:String>hello</x:String></x:Key></x:String>
  </ReadWriteDictionaryContainer.Dictionary>
</ReadWriteDictionaryContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ReadWriteNonSelfInstantiatingCollection()
        {
            var container = new ReadWriteNonSelfInstantiatingCollection() { Collection = new RunCollection() };
            container.Collection.Add(new MyRun { Text = "hello" });
            container.Collection.Add(new MyRun { Text = "world" });
            container.Collection.Add(new MyRun { Text = "moon" });
            container.Collection.Add(new MyRun { Text = "m  ars" });
            container.Collection.Add(new MyRun { Text = "venice" });
            string generated = SaveToString(container);
            string expected = 
@"<ReadWriteNonSelfInstantiatingCollection xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <ReadWriteNonSelfInstantiatingCollection.Collection>hello<MyRun>world</MyRun>moon<MyRun xml:space=""preserve"">m  ars</MyRun>venice</ReadWriteNonSelfInstantiatingCollection.Collection>
</ReadWriteNonSelfInstantiatingCollection>";
            
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ReadWriteCollectionWithFirstElementNull()
        {
            var c = new ReadWriteDictionaryContainer();
            c.Dictionary = new Hashtable();
            c.Dictionary.Add("hello", null);
            
            string generated = SaveToString(c);
            string expected = 
@"<ReadWriteDictionaryContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:sc=""clr-namespace:System.Collections;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ReadWriteDictionaryContainer.Dictionary>
    <sc:Hashtable>
      <x:Null>
        <x:Key>
          <x:String>hello</x:String>
        </x:Key>
      </x:Null>
    </sc:Hashtable>
  </ReadWriteDictionaryContainer.Dictionary>
</ReadWriteDictionaryContainer>";
            expected = string.Format(expected, typeof(Hashtable).GetAssemblyName());

            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void ContentWrapperAttribute()
        {
            var container = new RunCollectionContainer();

            container.Col.Add(new MyRun { Text = "hello" });
            container.Col.Add(new MyRun { Text = "world" });
            container.Col.Add(new MyRun { Text = "moon" });
            container.Col.Add(new MyRun { Text = "m  ars" });
            container.Col.Add(new MyRun { Text = "venice" });

            string generated = SaveToString(container);
            string expected = 
@"<RunCollectionContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <RunCollectionContainer.Col>hello<MyRun>world</MyRun>moon<MyRun xml:space=""preserve"">m  ars</MyRun>venice</RunCollectionContainer.Col>
</RunCollectionContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ITypeDescriptorContextInstance()
        {
            var inst = new UsesContextInstance { Value = "Hello" };
            var generated = XamlServices.Save(inst);
            var expected =
@"<UsesContextInstance Value=""Hello"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void PreventStealingXmlPrefix()
        {
            var o = new ObjectContainer { O = new X.M.L.A() };
            string generated = SaveToString(o);
            string expected =
@"<ObjectContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:p=""clr-namespace:X.M.L;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ObjectContainer.O>
    <p:A B=""{x:Null}"" />
  </ObjectContainer.O>
</ObjectContainer>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        [TestExpectedException(typeof(ArgumentException))]
        public void IllegalMemberName()
        {
            var o = new ClassWithInvalidMemberNameForXml() { Pro‿p1 = "something" };
            string generated = SaveToString(o);
        }

        [TestMethod]
        public void DictionaryKeyProperty()
        {
            var dict = new Dictionary<string, ClassWithDKP>();
            dict.Add("moon", new ClassWithDKP { DkpProp = "moon" });
            string generated = XamlServices.Save(dict);
            string expected =
@"<Dictionary x:TypeArguments=""x:String, te:ClassWithDKP"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <te:ClassWithDKP DkpProp=""moon"" />
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<string, ClassWithDKP>).GetAssemblyName());
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryKeyPropertyReadOnly()
        {
            var dict = new Dictionary<string, ClassWithReadOnlyDKP>();
            var o2 = new ClassWithReadOnlyDKP();
            o2.SetDkpProp("moon");
            dict.Add("moon", o2);
            string generated = XamlServices.Save(dict);
            string expected =
@"<Dictionary x:TypeArguments=""x:String, te:ClassWithReadOnlyDKP"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <te:ClassWithReadOnlyDKP x:Key=""moon"" />
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<string, ClassWithReadOnlyDKP>).GetAssemblyName());
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryKeyPropertyWithReference()
        {
            var dict = new Dictionary<string, ClassWithDKP>();
            var c = new ClassWithDKP { DkpProp = "world" };
            dict.Add("hello", c);
            dict.Add("world", c);
            dict.Add("moon", c);

            RoundtripObject<Dictionary<string, ClassWithDKP>>(dict, o =>
            {
                Assert.IsTrue(o["hello"] is ClassWithDKP);
                Assert.AreEqual(o["world"], o["hello"]);
                Assert.AreEqual(o["moon"], o["world"]);
            });
        }
        [TestMethod]
        public void CollectionPropertyWithPrivateSetter()
        {
            var o = new ClassWithCollectionWithPrivateSetter();
            string generated = SaveToString(o);
            string expected =
@"<ClassWithCollectionWithPrivateSetter xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ClassWithCollectionWithPrivateSetter.ArrayList>
    <x:Int32>1</x:Int32>
    <x:String>Hello</x:String>
  </ClassWithCollectionWithPrivateSetter.ArrayList>
</ClassWithCollectionWithPrivateSetter>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void GenericListWithDefaultValue()
        {
            var list = new ClassWithListPropertyAndDefaultValue();

            string generated = SaveToString(list);
            string expected = @"<ClassWithListPropertyAndDefaultValue xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ShouldSerializeGeneric()
        {
            var dictionary = new ClassWithDictionaryPropertyAndShouldSerialize();

            dictionary.Data.Add("hello", 1);

            string generated = SaveToString(dictionary);
            string expected = @"<ClassWithDictionaryPropertyAndShouldSerialize xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);

            dictionary.Data.Add("world", 2);
            RoundtripObject<ClassWithDictionaryPropertyAndShouldSerialize>(dictionary, o =>
            {
                Assert.AreEqual(o.Data["hello"], dictionary.Data["hello"]);
                Assert.AreEqual(o.Data["world"], dictionary.Data["world"]);
            });
        }

        [TestMethod]
        public void ReadOnlyCollectionCompatibility()
        {
            var o = new ClassWithListProperty();
            o.Data.Add("1");

            StringBuilder sb = new StringBuilder();
            var reader = new XamlObjectReader(o, new XamlObjectReaderSettings { RequireExplicitContentVisibility = true });
            var writer = new XamlXmlWriter(XmlWriter.Create(new StringWriter(sb)), reader.SchemaContext);
            XamlServices.Transform(reader, writer);

            var expected = @"<?xml version=""1.0"" encoding=""utf-16""?><ClassWithListProperty xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, sb.ToString());
        }

        [TestMethod]
        public void ConsecutiveSpaces()
        {
            string generated = XamlServices.Save("a  b");
            string expected = @"<x:String xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xml:space=""preserve"">a  b</x:String>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void SingleSpace()
        {
            string generated = XamlServices.Save("a b");
            string expected = @"<x:String xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">a b</x:String>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void SingleChar()
        {
            string generated = XamlServices.Save(@"a");
            string expected = @"<x:String xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">a</x:String>";
            Assert.AreEqual(generated, expected);
        }


        [TestMethod]
        public void SpecialCharAtTheEnd()
        {
            string generated = XamlServices.Save(@"Hello, World!
");
            string expected = @"<x:String xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xml:space=""preserve"">Hello, World!
</x:String>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void WhitespaceCharThatIsNotSpace()
        {
            string generated = XamlServices.Save(@"a
b");
            string expected = @"<x:String xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xml:space=""preserve"">a
b</x:String>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void SerializeStruct()
        {
            string generated = SaveToString(new MyStruct());
            string expected = @"<MyStruct Category=""{x:Null}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SerializeSimpleTypeAsProperty()
        {
            var o = new ObjectContainer { O = typeof(string) };
            string generated = XamlServices.Save(o);
            string expected = @"<ObjectContainer O=""{x:Type x:String}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SerializeTypeAsRoot()
        {
            string generated = XamlServices.Save(typeof(int));
            string expected = @"<x:Type Type=""x:Int32"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SerializeGenericType()
        {
            string generated = XamlServices.Save(typeof(List<Dictionary<int, string>>));

            string expected = @"<x:Type Type=""List(Dictionary(x:Int32, x:String))"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expected = string.Format(expected, typeof(Dictionary<int, string>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SerializeTypeAsProperty()
        {
            var o = new ObjectContainer { O = typeof(List<Dictionary<int, string>>) };
            string generated = XamlServices.Save(o);

            StringBuilder expected =
                new StringBuilder()
                .Append(@"<ObjectContainer O=""{x:Type &quot;scg:List(scg:Dictionary(x:Int32, x:String))&quot;}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=")
                .Append(typeof(List<Dictionary<int, string>>).GetAssemblyName())
                .Append(@""" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />");

            Assert.AreEqual(expected.ToString(), generated);
        }

        [TestMethod]
        public void SerializeTypeAsDictionaryMember()
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();
            dict.Add(typeof(Int32), 1);
            string generated = XamlServices.Save(dict);

            StringBuilder expected =
                new StringBuilder()
                .Append(@"<Dictionary x:TypeArguments=""x:Object, x:Object"" xmlns=""clr-namespace:System.Collections.Generic;assembly=")
                .Append(typeof(Dictionary<object, object>).GetAssemblyName())
                .Append(@""" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Int32 x:Key=""{x:Type x:Int32}"">1</x:Int32>
</Dictionary>");
            Assert.AreEqual(expected.ToString(), generated);
        }

        [TestMethod]
        public void SerializeTypeAsCollectionMember()
        {
            List<object> list = new List<object>();
            list.Add(typeof(Int32));
            string generated = XamlServices.Save(list);

            string expected =
@"<List x:TypeArguments=""x:Object"" Capacity=""4"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Type Type=""x:Int32"" />
</List>";
            expected = string.Format(expected, typeof(List<object>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void Eof()
        {
            var reader = new XamlObjectReader(new Object());

            Assert.IsFalse(reader.IsEof);
            Assert.IsTrue(reader.NodeType == XamlNodeType.None);

            while (true)
            {
                reader.Read();
                if (reader.IsEof)
                {
                    Assert.IsTrue(reader.NodeType == XamlNodeType.None);
                    Assert.IsTrue(reader.Type == null);
                    break;
                }
            }
        }

        [TestMethod]
        public void DictionaryLookAlike()
        {
            StringDictionary stringDictionary = new StringDictionary()
            {
                { "key", "value" }
            };
            var generated = SaveToString(stringDictionary);
            var expected =
@"<StringDictionary xmlns=""clr-namespace:System.Collections.Specialized;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""key"">value</x:String>
</StringDictionary>";
            expected = string.Format(expected, typeof(StringDictionary).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DependsOnSimple()
        {
            var o = new TryCatchFinally { Try = "1", Catch = "2", Finally = "3" };
            var generated = SaveToString(o);
            var expected = @"<TryCatchFinally Try=""1"" Catch=""2"" Finally=""3"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DependsOnWithContentProperty()
        {
            var o = new TryCatchFinallyWithContentProperty { Try = "1", Catch = "2", Finally = "3" };
            var generated = SaveToString(o);
            var expected = @"<TryCatchFinallyWithContentProperty Try=""1"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">2<TryCatchFinallyWithContentProperty.Finally>3</TryCatchFinallyWithContentProperty.Finally></TryCatchFinallyWithContentProperty>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DependsOnWithComplexDependencies()
        {
            var o = new Clothing { Coat = "", Gloves = "", Hat = "", Pants = "", Shirt = "", Tie = "", Socks = "", Underwear = "", Vest = "", Clone = "", Suit = "" };
            var generated = SaveToString(o);

            int clone = generated.IndexOf("Clone");
            int shirt = generated.IndexOf("Shirt");
            int underwear = generated.IndexOf("Underwear");
            int tie = generated.IndexOf("Tie");
            int pants = generated.IndexOf("Pants");
            int vest = generated.IndexOf("Vest");
            int suit = generated.IndexOf("Suit");
            int coat = generated.IndexOf("Coat");

            Assert.IsTrue(clone < coat);
            Assert.IsTrue(shirt < vest);
            Assert.IsTrue(vest < suit);
            Assert.IsTrue(suit < coat);
            Assert.IsTrue(pants < coat);
            Assert.IsTrue(underwear < pants);
        }

        [TestMethod]
        public void DependsOnWithComplexDependenciesWithMembersOmitted()
        {
            var o = new Clothing { Coat = "", Gloves = "", Hat = "", Pants = "", Shirt = "", Tie = "", Socks = "", Underwear = "", Clone = "" };
            var generated = SaveToString(o);

            int clone = generated.IndexOf("Clone");
            int shirt = generated.IndexOf("Shirt");
            int underwear = generated.IndexOf("Underwear");
            int tie = generated.IndexOf("Tie");
            int pants = generated.IndexOf("Pants");
            int vest = generated.IndexOf("Vest");
            int suit = generated.IndexOf("Suit");
            int coat = generated.IndexOf("Coat");

            Assert.IsTrue(clone < coat);
            Assert.IsTrue(shirt < coat);
            Assert.IsTrue(shirt < tie);
            Assert.IsTrue(pants < coat);
            Assert.IsTrue(underwear < pants);
        }

        [TestMethod]
        public void DependsOnWithComplexDependenciesAndComplexObjects()
        {
            var o = new ComplexClothing
            {
                Gloves = "",
                Clone = "",
                Coat = "4",
                Underwear = new ComplexType(),
                Hat = null,
                Socks = null,
                Shirt = "",
                Pants = "3",
                Tie = new ClassWithMEConverter(),
                Vest = "1",
                Suit = new ComplexType(),
            };
            var generated = SaveToString(o);

            int clone = generated.IndexOf("Clone");
            int shirt = generated.IndexOf("Shirt");
            int underwear = generated.IndexOf("Underwear");
            int tie = generated.IndexOf("Tie");
            int pants = generated.IndexOf("Pants");
            int vest = generated.IndexOf("Vest");
            int suit = generated.IndexOf("Suit");
            int coat = generated.IndexOf("Coat");
            int socks = generated.IndexOf("Socks");
            int gloves = generated.IndexOf("Gloves");
            int endOfRootElement = generated.IndexOf(">");

            int[] allClothes = new int[] { clone, shirt, underwear, tie, pants, vest, suit, coat, socks, gloves };
            foreach (int i in allClothes)
            {
                Assert.IsTrue(i > 0);
            }

            Assert.IsTrue(vest < suit);
            Assert.IsTrue(suit < coat);
            Assert.IsTrue(pants < coat);
            Assert.IsTrue(clone < coat);
            Assert.IsTrue(shirt < tie);
            Assert.IsTrue(tie < vest);
            Assert.IsTrue(underwear < pants);

            Assert.IsTrue(socks < endOfRootElement);
            Assert.IsTrue(clone < endOfRootElement);
            Assert.IsTrue(gloves < endOfRootElement);
            Assert.IsTrue(shirt < endOfRootElement);
            Assert.IsTrue(tie < endOfRootElement);
            Assert.IsTrue(vest < endOfRootElement);
        }

        [TestMethod]
        public void CircularDependency()
        {
            var o = new CircularDependency() { Chicken = "", Egg = "" };
            var generated = SaveToString(o);
        }

        [TestMethod]
        public void TypeConverterOnGrandparent()
        {
            var generated = SaveToString(new ClassWithTCOnGrandparent());
            var expected = @"<ClassWithTCOnGrandparent xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">0</ClassWithTCOnGrandparent>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void CollectionWithNoXamlSerializableMember()
        {
            var annotations = new Annotations();
            annotations.Add(new Annotation());

            var generated = XamlServices.Save(annotations);
            var expected =
@"<Annotations xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <Annotation JobId=""0"" />
</Annotations>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryWithNullKey()
        {
            var dict = new DictionaryAllowingNullKey();
            dict.Add(null, "bye");

            var expected =
@"<DictionaryAllowingNullKey xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""{x:Null}"">bye</x:String>
</DictionaryAllowingNullKey>";
            var generated = XamlServices.Save(dict);

            Assert.AreEqual(expected, generated);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void MultidimensionalArray()
        {
            var obj = new char[2, 2] { { 'a', 'b' }, { 'c', 'd' } };
            var generated = XamlServices.Save(obj);
            var expected =
@"<x:Array Type=""Char"" xmlns=""clr-namespace:System;assembly=mscorlib"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <Char>a</Char>
  <Char>b</Char>
  <Char>c</Char>
  <Char>d</Char>
</x:Array>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MultidimensionalJaggedArray()
        {
            int[][] obj = 
            {
                new int[] {1,3,5,7},
                new int[] {0,2,4},
                new int[] {11,22}
            };

            var generated = XamlServices.Save(obj);
            var expected =
@"<x:Array Type=""Int32[]"" xmlns=""clr-namespace:System;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Array Type=""x:Int32"">
    <x:Int32>1</x:Int32>
    <x:Int32>3</x:Int32>
    <x:Int32>5</x:Int32>
    <x:Int32>7</x:Int32>
  </x:Array>
  <x:Array Type=""x:Int32"">
    <x:Int32>0</x:Int32>
    <x:Int32>2</x:Int32>
    <x:Int32>4</x:Int32>
  </x:Array>
  <x:Array Type=""x:Int32"">
    <x:Int32>11</x:Int32>
    <x:Int32>22</x:Int32>
  </x:Array>
</x:Array>";
            expected = string.Format(expected, typeof(int[]).GetAssemblyName());
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ObjectPropertyTest()
        {
            DictionaryContainer d = new DictionaryContainer();
            d.Dict = new Dictionary<string, DictionaryContainer>();
            d.Dict.Add("Hello", d);

            var reader = new XamlObjectReader(d);
            while (reader.Read() && reader.NodeType != XamlNodeType.StartObject) { }
            Assert.IsTrue(reader.Instance is DictionaryContainer);
            Assert.IsNotNull(reader.Instance);
            var obj = reader.Instance;

            while (reader.Read() && reader.NodeType != XamlNodeType.StartObject) { }
            Assert.IsTrue(reader.Instance is Dictionary<string, DictionaryContainer>);
            Assert.IsNotNull(reader.Instance);

            while (reader.Read() && reader.NodeType != XamlNodeType.StartObject) { }
            Assert.IsTrue(reader.Instance == obj);
        }

        [TestMethod]
        public void ObjectPropertyTestOnTypeConverted()
        {
            TigerContainer container = new TigerContainer { Tiger = new Tiger("Woods") };
            var reader = new XamlObjectReader(container);
            while (reader.Read() && reader.NodeType != XamlNodeType.StartObject) { };
            reader.Read();
            Assert.IsNull(reader.Instance);

            reader.Read();
            Assert.IsNull(reader.Instance);
        }

        [TestMethod]
        public void InheritedPropertiesInDifferentNamespace()
        {
            var de = new DerivedElement { Element = new Element() };
            string generated = XamlServices.Save(de);
            string expected =
@"<DerivedElement xmlns=""clr-namespace:Test.Elements.Derived;assembly=XamlTestClasses"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <DerivedElement.Element>
    <te:Element />
  </DerivedElement.Element>
</DerivedElement>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ArrayListInt()
        {
            ArrayList list = new ArrayList { 1, 2, 3 };
            string generated = SaveToString(list);
            string expected =
@"<ArrayList Capacity=""4"" xmlns=""clr-namespace:System.Collections;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Int32>1</x:Int32>
  <x:Int32>2</x:Int32>
  <x:Int32>3</x:Int32>
</ArrayList>";
            expected = string.Format(expected, typeof(ArrayList).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ListOfTypeConverted()
        {
            List<ClassWithTypeConverter> list = new List<ClassWithTypeConverter>();
            list.Add(new ClassWithTypeConverter(1));
            list.Add(new ClassWithTypeConverter(2));
            list.Add(new ClassWithTypeConverter(3));

            string generated = SaveToString(list);

            string expected =
@"<List x:TypeArguments=""te:ClassWithTypeConverter"" Capacity=""4"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <te:ClassWithTypeConverter>1</te:ClassWithTypeConverter>
  <te:ClassWithTypeConverter>2</te:ClassWithTypeConverter>
  <te:ClassWithTypeConverter>3</te:ClassWithTypeConverter>
</List>";
            expected = string.Format(expected, typeof(List<ClassWithTypeConverter>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ListStringAsProperty()
        {
            var cwlp = new ClassWithListProperty();

            var expected = @"<ClassWithListProperty xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            var generated = SaveToString(cwlp);
            Assert.AreEqual(expected, generated);

            cwlp.Data.Add("Hello");
            cwlp.Data.Add("World");

            expected = @"<ClassWithListProperty xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ClassWithListProperty.Data>
    <x:String>Hello</x:String>
    <x:String>World</x:String>
  </ClassWithListProperty.Data>
</ClassWithListProperty>";
            generated = SaveToString(cwlp);
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void SerializeArray()
        {
            Int16 a = 1;
            Byte b = 2;
            TimeSpan c = new TimeSpan(3);
            Int64 d = 3;
            Uri e = new Uri("http://woot.com");
            Decimal f = new Decimal(4);
            Single g = 5.6f;

            object[] o = new object[] { a, b, c, d, e, f, g };

            string expected =
@"<x:Array Type=""x:Object"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Int16>1</x:Int16>
  <x:Byte>2</x:Byte>
  <x:TimeSpan>00:00:00.0000003</x:TimeSpan>
  <x:Int64>3</x:Int64>
  <x:Uri>http://woot.com</x:Uri>
  <x:Decimal>4</x:Decimal>
  <x:Single>5.6</x:Single>
</x:Array>";
            var generated = SaveToString(o);

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryIntString()
        {
            IDictionary<int, string> data = new Dictionary<int, string>();

            data.Add(1, "one");

            var expected =
@"<Dictionary x:TypeArguments=""x:Int32, x:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""1"">one</x:String>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<int, string>).GetAssemblyName());

            var generated = SaveToString(data);
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryStringString()
        {
            IDictionary<string, string> data = new Dictionary<string, string>();

            data.Add("one", "1");

            var expected = 
@"<Dictionary x:TypeArguments=""x:String, x:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""one"">1</x:String>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<string, string>).GetAssemblyName());

            var generated = SaveToString(data);
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryStringStringArray()
        {
            IDictionary<string, string[]> data = new Dictionary<string, string[]>();

            data.Add("one", new string[] { "1", "2", "3" });

            var expected = 
@"<Dictionary x:TypeArguments=""x:String, s:String[]"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:s=""clr-namespace:System;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Array x:Key=""one"" Type=""x:String"">
    <x:String>1</x:String>
    <x:String>2</x:String>
    <x:String>3</x:String>
  </x:Array>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<string, string[]>).GetAssemblyName());

            var generated = SaveToString(data);
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryIComparableString()
        {
            IDictionary<IComparable, string> data = new Dictionary<IComparable, string>();

            data.Add(1, "one");
            data.Add("2", "two");

            RoundtripObject<IDictionary<IComparable, string>>(data, o =>
            {
                Assert.AreEqual(o[1], data[1]);
                Assert.AreEqual(o["2"], data["2"]);
            });
        }

        [TestMethod]
        public void ReadOnlyDictionary()
        {
            ClassWithDictionaryProperty data = new ClassWithDictionaryProperty();
            var generated = SaveToString(data);
            var expected = @"<ClassWithDictionaryProperty xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);

            data.Data.Add("Hello", 1);

            generated = SaveToString(data);
            expected = @"<ClassWithDictionaryProperty xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ClassWithDictionaryProperty.Data>
    <x:Int32 x:Key=""Hello"">1</x:Int32>
  </ClassWithDictionaryProperty.Data>
</ClassWithDictionaryProperty>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DefaultValue()
        {
            var obj = new ClassWithPropertyWithDefaultValueOfNull();
            var xaml = SaveToString(obj);
            var expected = @"<ClassWithPropertyWithDefaultValueOfNull Prop2=""{x:Null}"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, xaml);

            obj.Prop1 = null;
            obj.Prop2 = null;
            xaml = SaveToString(obj);
            Assert.AreEqual(expected, xaml);

            obj.Prop1 = "Hello";
            obj.Prop2 = "World";
            xaml = SaveToString(obj);
            expected = @"<ClassWithPropertyWithDefaultValueOfNull Prop1=""Hello"" Prop2=""World"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, xaml);
        }

        [TestMethod]
        public void DefaultValueByTypeConverterTest()
        {
            //instantiate an object with non-default value set
            DefaultValueTestByTypeConverterClass obj = new DefaultValueTestByTypeConverterClass(new ClassWithTypeConverter(200));
            var result = SaveToString(obj);

            var expected = @"<DefaultValueTestByTypeConverterClass Data=""200"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, result);

            //instantiate an object with default value set
            obj = new DefaultValueTestByTypeConverterClass(new ClassWithTypeConverter(100));
            result = SaveToString(obj);

            expected = @"<DefaultValueTestByTypeConverterClass xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RecursiveReference()
        {
            RecursiveReference r = new RecursiveReference();
            string generated = SaveToString(r);
            var expected =
@"<RecursiveReference RR=""{x:Reference __ReferenceID0}"" x:Name=""__ReferenceID0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void CyclicReferenceWithRuntimeNameAsContentProperty()
        {
            Person a = new Person
            {
                Age = 1,
                Name = "AngleBracket"
            };
            Person p = new Person
            {
                Age = 29,
                Name = "Daniel Roth",
                Friends = { a },
            };
            a.Friends.Add(p);

            var output = SaveToString(p);

            // @TODO, we seem to have a bug with writing out the content property mixed with other stuff, here
            //        I would expect to see the names on their own lines.
            var expected =
@"<Person Age=""29"" Name=""Daniel Roth"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <Person.Friends>
    <Person Age=""1"" Name=""AngleBracket"">
      <Person.Friends>
        <x:Reference>Daniel Roth</x:Reference>
      </Person.Friends>
    </Person>
  </Person.Friends>
</Person>";
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void NameScopeWithVisibleReference()
        {
            var foo = new Simple { Prop = "some string" };
            var ns = new NameScope { Content = foo };
            var ns2 = new NameScope { Content = foo };
            var arr = new object[] { ns, ns2, foo };

            var generated = SaveToString(arr);
            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope>
    <x:Reference>__ReferenceID0</x:Reference>
  </NameScope>
  <NameScope>
    <x:Reference>__ReferenceID0</x:Reference>
  </NameScope>
  <Simple x:Name=""__ReferenceID0"" Prop=""some string"" />
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameScopeWithoutVisibleReferenceFail()
        {
            var foo = new Simple { Prop = "some string" };
            var ns = new NameScope { Content = foo };
            var ns2 = new NameScope { Content = foo };
            var arr = new object[] { ns, ns2 };

            string generated = SaveToString(arr);
            string expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope>
    <Simple Prop=""some string"" />
  </NameScope>
  <NameScope>
    <Simple Prop=""some string"" />
  </NameScope>
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameScopeTwoDeepWithVisibleReference()
        {
            var foo = new Simple { Prop = "some string" };
            var ns2 = new NameScope { Content = foo };
            var ns = new NameScope { Content = ns2 };
            var arr = new object[] { ns, ns2, foo };

            var generated = SaveToString(arr);
            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope>
    <x:Reference>__ReferenceID0</x:Reference>
  </NameScope>
  <NameScope x:Name=""__ReferenceID0"">
    <x:Reference>__ReferenceID1</x:Reference>
  </NameScope>
  <Simple x:Name=""__ReferenceID1"" Prop=""some string"" />
</x:Array>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NameScopeWithCyclicReference()
        {
            var foo = new Simple { Prop = "some string" };
            var ns2 = new NameScope();
            var ns = new NameScope();
            ns.Content = new object[] { foo, ns2 };
            ns2.Content = new object[] { foo, ns };
            var arr = new object[] { ns };

            var generated = SaveToString(arr);
            var expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope x:Name=""__ReferenceID1"">
    <x:Array Type=""x:Object"">
      <Simple x:Name=""__ReferenceID0"" Prop=""some string"" />
      <NameScope>
        <x:Array Type=""x:Object"">
          <x:Reference>__ReferenceID0</x:Reference>
          <x:Reference>__ReferenceID1</x:Reference>
        </x:Array>
      </NameScope>
    </x:Array>
  </NameScope>
</x:Array>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void NameScopeWithCyclicReferenceFail()
        {
            var foo = new Simple { Prop = "some string" };
            var ns2 = new NameScope();
            var ns = new NameScope();
            ns.Content = new object[] { foo, ns2 };
            ns2.Content = new object[] { foo, ns };
            var arr = new object[] { ns, ns2 };

            string generated = SaveToString(arr);
            string expected =
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <NameScope x:Name=""__ReferenceID1"">
    <x:Array Type=""x:Object"">
      <Simple Prop=""some string"" />
      <x:Reference>__ReferenceID0</x:Reference>
    </x:Array>
  </NameScope>
  <NameScope x:Name=""__ReferenceID0"">
    <x:Array Type=""x:Object"">
      <Simple Prop=""some string"" />
      <x:Reference>__ReferenceID1</x:Reference>
    </x:Array>
  </NameScope>
</x:Array>";

            Assert.AreEqual(generated, expected);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void RuntimeNameWithConflictingNamesInSameScope()
        {
            var namedThings = new FooNamed[]
                {
                    new FooNamed { Name = "Josh" },
                    new FooNamed { Name = "Josh" },
                };

            using (var tw = new StringWriter())
            {
                XamlServices.Save(tw, namedThings);
            }
        }

        [TestMethod]
        public void RuntimeNameWithConflictingNamesInDifferentScope()
        {
            var namedThings = new object[]
                {
                    new FooNamed { Name = "Josh" },
                    new NameScope { Content = new FooNamed { Name = "Josh" } },
                };

            var tw = new StringWriter();
            using (var xw = XmlWriter.Create(tw, new XmlWriterSettings { Indent = true }))
            {
                XamlServices.Save(tw, namedThings);
            }
        }

        [TestMethod]
        public void ReferenceInDictionary()
        {
            DictionaryContainer d = new DictionaryContainer();
            d.Dict = new Dictionary<string, DictionaryContainer>();
            d.Dict.Add("Hello", d);
            var generated = SaveToString(d);

            var expected =
@"<DictionaryContainer x:Name=""__ReferenceID0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <DictionaryContainer.Dict>
    <scg:Dictionary x:TypeArguments=""x:String, DictionaryContainer"">
      <x:Reference>__ReferenceID0<x:Key>Hello</x:Key></x:Reference>
    </scg:Dictionary>
  </DictionaryContainer.Dict>
</DictionaryContainer>";
            expected = string.Format(expected, typeof(Dictionary<string, DictionaryContainer>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void RuntimeNameWithConflictingNamesInSubScope()
        {
            var namedThings = new object[]
                {
                    new FooNamed { Name = "Josh" },
                    new NameScope
                    {
                        Content = new object[] { new FooNamed { Name = "Josh" }, new FooNamed { Name = "Josh" } }
                    },
                };

            var tw = new StringWriter();
            using (var xw = XmlWriter.Create(tw, new XmlWriterSettings { Indent = true }))
            {
                XamlServices.Save(tw, namedThings);
            }
        }

        [TestMethod]
        public void LookupPrefixInTypeConverter()
        {
            XNameToyContainer container = new XNameToyContainer() { XName = new XNameToy { Namespace = "http://hello", LocalName = "world" } };
            var generated = SaveToString(container);
            var expected = @"<XNameToyContainer XName=""p:world"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:p=""http://hello"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsInstanceDescriptor()
        {
            PointWithNoCtorArgAttrs obj = new PointWithNoCtorArgAttrs(1, 2);
            var generated = SaveToString(obj);
            var expected =
@"<PointWithNoCtorArgAttrs xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
  </x:Arguments>
</PointWithNoCtorArgAttrs>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsIncomplete()
        {
            PointWithIncompleteConstructor obj = new PointWithIncompleteConstructor(1, 2);
            obj.Z = 3;
            var generated = SaveToString(obj);
            var expected =
@"<PointWithIncompleteConstructor Z=""3"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
  </x:Arguments>
</PointWithIncompleteConstructor>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsInstanceDescriptorNested()
        {
            NestedPointWithNoCtorArgAttrs obj = new NestedPointWithNoCtorArgAttrs(1, 2, new PointWithNoCtorArgAttrs(10, 20));
            var generated = SaveToString(obj);
            var expected =
@"<NestedPointWithNoCtorArgAttrs xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
    <PointWithNoCtorArgAttrs>
      <x:Arguments>
        <x:Int32>10</x:Int32>
        <x:Int32>20</x:Int32>
      </x:Arguments>
    </PointWithNoCtorArgAttrs>
  </x:Arguments>
</NestedPointWithNoCtorArgAttrs>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        [TestExpectedException(typeof(XamlObjectReaderException))]
        public void ConstructorArgumentsNoMatch()
        {
            var obj = new PointWithNoMatchingConstructor(1, 2);
            SaveToString(obj);
        }

        [TestMethod]
        [TestExpectedException(typeof(XamlObjectReaderException))]
        public void ConstructorArgumentsNoMatchIncorrectTypes()
        {
            var obj = new PointWithNoMatchingConstructorIncorrectTypes("1", "2");
            SaveToString(obj);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void InstanceDescriptorOutOfOrder()
        {
            DifferentArgumentMethodClass obj = DifferentArgumentMethodClass.MyMethod
                (100, new Dictionary<int, int> { { 1, 2 }, { 3, 4 } }, new List<int> { 1000, 2000 }, new int[] { 1, 2, 3, 4, 5 }, new List<int> { 1000, 2000 }, 10);

            SaveToString(obj);
        }

        [TestMethod]
        public void ConstructorArgumentsMultipleDisjointCtors()
        {
            BadClassWithCleverTypeConverter bcwctc = new BadClassWithCleverTypeConverter(1, 2, 3);
            bcwctc.X = "Goodbye";
            bcwctc.Y = "Cruel";
            bcwctc.Z = "World";
            var generated = SaveToString(bcwctc);
            var expected =
@"<BadClassWithCleverTypeConverter X=""Goodbye"" Y=""Cruel"" Z=""World"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
    <x:Int32>3</x:Int32>
  </x:Arguments>
</BadClassWithCleverTypeConverter>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        [TestExpectedException(typeof(XamlObjectReaderException))]
        public void ConstructorArgumentsBadCasing()
        {
            var obj = new PointWithBadCasing(1, 2);
            SaveToString(obj);
        }


        [TestMethod]
        [TestExpectedException(typeof(XamlObjectReaderException), typeof(Exception))]
        public void InstanceDescriptorTypeConverterOnProperty()
        {
            SimpleClass2 sc = new SimpleClass2 { B = new SimpleClass1(3, "blah"), A = "blah" };
            SaveToString(sc);
        }

        [TestMethod]
        public void StaticMethodNoCtor()
        {
            PointWithStaticMethod obj = PointWithStaticMethod.Create(1, 2);
            var generated = SaveToString(obj);
            var expected =
@"<PointWithStaticMethod x:FactoryMethod=""Create"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
  </x:Arguments>
</PointWithStaticMethod>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryOfTypeConverted()
        {
            Dictionary<ClassWithTypeConverter, string> dict = new Dictionary<ClassWithTypeConverter, string>();
            dict.Add(new ClassWithTypeConverter(1), "a");
            dict.Add(new ClassWithTypeConverter(2), "b");
            dict.Add(new ClassWithTypeConverter(3), "c");

            string generated = SaveToString(dict);

            string expected =
@"<Dictionary x:TypeArguments=""te:ClassWithTypeConverter, x:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""1"">a</x:String>
  <x:String x:Key=""2"">b</x:String>
  <x:String x:Key=""3"">c</x:String>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<ClassWithTypeConverter, string>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryWithComplexKeyAndSimpleValue()
        {
            IDictionary<PointWithStaticMethod, string> v = new Dictionary<PointWithStaticMethod, string>();

            var pt = PointWithStaticMethod.Create(10, 10);

            v.Add(pt, "uno");

            pt = PointWithStaticMethod.Create(10, 20);

            v.Add(pt, "dous");

            pt = PointWithStaticMethod.Create(10, 30);

            v.Add(pt, "tres");

            var expected =
@"<Dictionary x:TypeArguments=""te:PointWithStaticMethod, x:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>uno<x:Key><te:PointWithStaticMethod x:FactoryMethod=""Create""><x:Arguments><x:Int32>10</x:Int32><x:Int32>10</x:Int32></x:Arguments></te:PointWithStaticMethod></x:Key></x:String>
  <x:String>dous<x:Key><te:PointWithStaticMethod x:FactoryMethod=""Create""><x:Arguments><x:Int32>10</x:Int32><x:Int32>20</x:Int32></x:Arguments></te:PointWithStaticMethod></x:Key></x:String>
  <x:String>tres<x:Key><te:PointWithStaticMethod x:FactoryMethod=""Create""><x:Arguments><x:Int32>10</x:Int32><x:Int32>30</x:Int32></x:Arguments></te:PointWithStaticMethod></x:Key></x:String>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<PointWithStaticMethod, string>).GetAssemblyName());

            var generated = SaveToString(v);
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryWithComplexKeyAndComplexValue()
        {
            IDictionary<PointWithStaticMethod, PointWithStaticMethod> v = new Dictionary<PointWithStaticMethod, PointWithStaticMethod>();

            var pt = PointWithStaticMethod.Create(10, 10);
            var val = PointWithStaticMethod.Create(10, 10);
            v.Add(pt, val);

            pt = PointWithStaticMethod.Create(10, 20);
            val = PointWithStaticMethod.Create(20, 20);

            v.Add(pt, val);

            pt = PointWithStaticMethod.Create(10, 30);
            val = PointWithStaticMethod.Create(30, 10);

            v.Add(pt, val);

            var expected =
@"<Dictionary x:TypeArguments=""te:PointWithStaticMethod, te:PointWithStaticMethod"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <te:PointWithStaticMethod x:FactoryMethod=""Create"">
    <x:Arguments>
      <x:Int32>10</x:Int32>
      <x:Int32>10</x:Int32>
    </x:Arguments>
    <x:Key>
      <te:PointWithStaticMethod x:FactoryMethod=""Create"">
        <x:Arguments>
          <x:Int32>10</x:Int32>
          <x:Int32>10</x:Int32>
        </x:Arguments>
      </te:PointWithStaticMethod>
    </x:Key>
  </te:PointWithStaticMethod>
  <te:PointWithStaticMethod x:FactoryMethod=""Create"">
    <x:Arguments>
      <x:Int32>20</x:Int32>
      <x:Int32>20</x:Int32>
    </x:Arguments>
    <x:Key>
      <te:PointWithStaticMethod x:FactoryMethod=""Create"">
        <x:Arguments>
          <x:Int32>10</x:Int32>
          <x:Int32>20</x:Int32>
        </x:Arguments>
      </te:PointWithStaticMethod>
    </x:Key>
  </te:PointWithStaticMethod>
  <te:PointWithStaticMethod x:FactoryMethod=""Create"">
    <x:Arguments>
      <x:Int32>30</x:Int32>
      <x:Int32>10</x:Int32>
    </x:Arguments>
    <x:Key>
      <te:PointWithStaticMethod x:FactoryMethod=""Create"">
        <x:Arguments>
          <x:Int32>10</x:Int32>
          <x:Int32>30</x:Int32>
        </x:Arguments>
      </te:PointWithStaticMethod>
    </x:Key>
  </te:PointWithStaticMethod>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<PointWithStaticMethod, PointWithStaticMethod>).GetAssemblyName());

            var generated = SaveToString(v);
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsWithNull()
        {
            ConstructorArgTypes1 cat = new ConstructorArgTypes1(12, null);
            var generated = SaveToString(cat);
            var expected =
@"<ConstructorArgTypes1 xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>12</x:Int32>
    <x:Null />
  </x:Arguments>
</ConstructorArgTypes1>";
            Assert.AreEqual(expected, generated);

            cat = new ConstructorArgTypes1(12, "some string");
            generated = SaveToString(cat);
            expected =
@"<ConstructorArgTypes1 xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>12</x:Int32>
    <x:String>some string</x:String>
  </x:Arguments>
</ConstructorArgTypes1>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsWithNullable()
        {
            ConstructorArgTypes5 cat = new ConstructorArgTypes5(null);
            var generated = SaveToString(cat);
            var expected =
@"<ConstructorArgTypes5 xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Null />
  </x:Arguments>
</ConstructorArgTypes5>";
            Assert.AreEqual(expected, generated);

            cat = new ConstructorArgTypes5(100);
            generated = SaveToString(cat);
            expected =
@"<ConstructorArgTypes5 xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>100</x:Int32>
  </x:Arguments>
</ConstructorArgTypes5>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsMultipleCtors()
        {
            PointWithMultipleMatchingConstructors obj = new PointWithMultipleMatchingConstructors(1, 2);
            var generated = SaveToString(obj);
            var expected =
@"<PointWithMultipleMatchingConstructors xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
  </x:Arguments>
</PointWithMultipleMatchingConstructors>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructorArgumentsWithTypeConverter()
        {
            BarsComeFromFoos b = new BarsComeFromFoos(new Foos { A = 12, B = 10023 });

            var generated = SaveToString(b);
            var expected = @"<BarsComeFromFoos xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <FoosHelper SomeA=""12"" SomeB=""10023"" />
  </x:Arguments>
</BarsComeFromFoos>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void StringTest()
        {
            string s = "SomeString";
            StringWriter sw;
            using (sw = new StringWriter())
            {
                using (var xw = XmlWriter.Create(sw, new XmlWriterSettings() { Indent = true }))
                {
                    XamlServices.Save(xw, s);
                }
            }
        }

        [TestMethod]
        public void ListWithStringEmpty()
        {
            var strings = new List<string> { "Whatever", String.Empty, "Another thing", null };

            var generated = SaveToString(strings);

            var expected =
@"<List x:TypeArguments=""x:String"" Capacity=""4"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>Whatever</x:String>
  <x:String></x:String>
  <x:String>Another thing</x:String>
  <x:Null />
</List>";
            expected = string.Format(expected, typeof(List<string>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void IComparableTest()
        {
            var stringAsIComparable = new ContainerOfIComparable { X = "Hello" };
            RoundtripObject(stringAsIComparable, o =>
            {
                Assert.AreEqual("Hello", o.X);
            });

            var complexTypeAsIComparable = new ContainerOfIComparable { X = new DummyIComparableImpl { Y = "World" } };

            RoundtripObject(complexTypeAsIComparable, o =>
            {
                Assert.AreEqual("World", ((DummyIComparableImpl)o.X).Y);
            });
        }

        [TestMethod]
        public void ListStringAsCPA()
        {
            var cwlp = new ClassWithListPropertyAsCPA();
            var generated = SaveToString(cwlp);

            var expected = @"<ClassWithListPropertyAsCPA xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);

            cwlp.Data.Add("Hello");
            cwlp.Data.Add("World");

            generated = SaveToString(cwlp);
            expected = @"<ClassWithListPropertyAsCPA xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String>Hello</x:String>
  <x:String>World</x:String>
</ClassWithListPropertyAsCPA>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void CollectionConverterContentPropertyTest()
        {
            Zoo2 zoo = new Zoo2();
            Animal tiger = new Animal();
            tiger.Name = "Tiger";
            tiger.Number = 2;
            Animal monkey = new Animal();
            monkey.Name = "Monkey";
            monkey.Number = 3;
            zoo.Animals.Add(tiger);
            zoo.Animals.Add(monkey);

            var generated = SaveToString(zoo);
            var expected = @"<Zoo2 Animals=""Tiger:2#Monkey:3"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void FactoryPattern()
        {
            PointWithFactory pt = PointFactory.CreatePoint(10, 20);
            FactoryPattern_Test(pt);
            RoundtripObject_String(pt, FactoryPattern_Test);
        }
        void FactoryPattern_Test(object obj)
        {
            PointWithFactory pt = obj as PointWithFactory;
            Assert.IsNotNull(pt, "obj should be a PointWithFactory");
            Assert.AreEqual(10, pt.X, "pt.X should be 10");
            Assert.AreEqual(20, pt.Y, "pt.Y should be 20");
        }

        [TestMethod]
        public void DictionaryXNameString()
        {
            IDictionary<XNameToy, string> data = new Dictionary<XNameToy, string>();

            data.Add(new XNameToy { Namespace = "http://example.org/test", LocalName = "one" }, "1");

            var generated = SaveToString(data);

            var expected =
@"<Dictionary x:TypeArguments=""te:XNameToy, x:String"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:p=""http://example.org/test"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""p:one"">1</x:String>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<XNameToy, string>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryXNameStringAsCPA()
        {
            var cwdp = new ClassWithDictionaryPropertyAsCPA();

            cwdp.Data.Add(new XNameToy { Namespace = "http://example.org/test", LocalName = "one" }, "1");

            var generated = SaveToString(cwdp);
            var expected =
@"<ClassWithDictionaryPropertyAsCPA xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:p=""http://example.org/test"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:String x:Key=""p:one"">1</x:String>
</ClassWithDictionaryPropertyAsCPA>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DictionaryXNameXName()
        {
            var data = new Dictionary<XNameToy, XNameToy>();

            data.Add(new XNameToy { Namespace = "http://example.org/test", LocalName = "one" }, new XNameToy { Namespace = "http://example.org/test2", LocalName = "uno" });

            string generated = SaveToString(data);

            var expected = 
@"<Dictionary x:TypeArguments=""te:XNameToy, te:XNameToy"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:p=""http://example.org/test2"" xmlns:p1=""http://example.org/test"" xmlns:te=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <te:XNameToy x:Key=""p1:one"">p:uno</te:XNameToy>
</Dictionary>";
            expected = string.Format(expected, typeof(Dictionary<XNameToy, XName>).GetAssemblyName());

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void MarkupExtensionAtRoot()
        {
            var f = new Foos { A = 12, B = 10023 };

            var expected = @"<FoosHelper SomeA=""12"" SomeB=""10023"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            var generated = SaveToString(f);
            Assert.AreEqual(expected, generated);

            var arr = new object[] { f };
            generated = SaveToString(arr);
            expected = 
@"<x:Array Type=""x:Object"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <FoosHelper SomeA=""12"" SomeB=""10023"" />
</x:Array>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void EmptyReadWriteCollection()
        {
            List<int> list = new List<int>();
            string generated = XamlServices.Save(list);

            var expected = @"<List x:TypeArguments=""x:Int32"" Capacity=""0"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expected = string.Format(expected, typeof(List<int>).GetAssemblyName());
            Assert.AreEqual(expected, generated);

            Dictionary<int, int> dict = new Dictionary<int, int>();
            generated = XamlServices.Save(list);

            expected = @"<List x:TypeArguments=""x:Int32"" Capacity=""0"" xmlns=""clr-namespace:System.Collections.Generic;assembly={0}"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            expected = string.Format(expected, typeof(List<int>).GetAssemblyName());
            Assert.AreEqual(expected, generated);

            Foos[] array = new Foos[0];
            generated = XamlServices.Save(array);

            expected = @"<x:Array Type=""Foos"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ConstructionDirectivesOrdering()
        {
            var p = PointWithStaticMethod2.Create(1, 2);

            var generated = SaveToString(p);
            var expected =
@"<PointWithStaticMethod2 x:FactoryMethod=""Create"" X=""1"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:Int32>1</x:Int32>
    <x:Int32>2</x:Int32>
  </x:Arguments>
</PointWithStaticMethod2>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void NestedType()
        {
            SaveToString(new Outer.Inner());
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void MemberOfNestedType()
        {
            SaveToString(new Outer { InnerMember = new Outer.Inner() });
        }

        [TestMethod]
        public void MemberAssignableToNestedType()
        {
            var generated = SaveToString(new Outer { InnerMember = new InnerChild() });
            var expected =
@"<Outer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <Outer.InnerMember>
    <InnerChild />
  </Outer.InnerMember>
</Outer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void NullReadOnlyDictionaryProperty()
        {
            var generated = SaveToString(new ClassWithDictionaryProperty());
            var expected = @"<ClassWithDictionaryProperty xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void GenericArray()
        {
            var genericArray = new SimpleGenericType<int>[2];
            genericArray[0] = new SimpleGenericType<int> { Info1 = 1 };
            genericArray[1] = new SimpleGenericType<int> { Info1 = 2 };

            var generated = SaveToString(genericArray);
            var expected =
@"<x:Array Type=""SimpleGenericType(x:Int32)"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <SimpleGenericType x:TypeArguments=""x:Int32"" Info1=""1"" />
  <SimpleGenericType x:TypeArguments=""x:Int32"" Info1=""2"" />
</x:Array>";
            Assert.AreEqual(expected, generated);

            genericArray = new SimpleGenericType<int>[0];
            generated = SaveToString(genericArray);
            expected = @"<x:Array Type=""SimpleGenericType(x:Int32)"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void TypeConvertToNull()
        {
            TigerContainer zoo = new TigerContainer { Tiger = new Tiger(null) };
            string generated = SaveToString(zoo);
            var expected = @"<TigerContainer Tiger="""" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void TypeConvertToNullAtRoot()
        {
            string generated = SaveToString(new Tiger(null));
            var expected =
@"<Tiger xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses""></Tiger>";

            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void ArrayAttachedProperty()
        {
            object o = new object();
            ArrayAttachedPropertyOwner.SetStrings(o, new string[] { "foo", "bar" });

            RoundtripObject(o, o1 =>
            {
                String[] strings = ArrayAttachedPropertyOwner.GetStrings(o1);

                Assert.IsNotNull(strings);
                Assert.AreEqual(2, strings.Length);
                Assert.AreEqual("foo", strings[0]);
                Assert.AreEqual("bar", strings[1]);
            });
        }

        [TestMethod]
        public void CollectionConverterOnAttachedProperty()
        {
            object f = new Object();
            var animals = new List<Animal>
                {
                    new Animal
                    {
                        Name = "Tiger",
                        Number = 17
                    },
                    new Animal
                    {
                        Name = "Monkey",
                        Number = 3
                    }
                };
            ZooRUs.SetAnimals(f, animals);
            Assert.IsTrue(ZooRUs.GetAnimals(f).Count == 2);

            var expected = @"<x:Object ZooRUs.Animals=""Tiger:17#Monkey:3"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";
            var generated = SaveToString(f);
            Assert.AreEqual(expected, generated);

            RoundtripObject(f, f2 =>
            {
                var fanimals = ZooRUs.GetAnimals(f);
                var f2animals = ZooRUs.GetAnimals(f2);
                Assert.AreEqual(fanimals.Count, f2animals.Count);
                Assert.AreEqual(fanimals[0].Name, f2animals[0].Name);
                Assert.AreEqual(fanimals[1].Number, f2animals[1].Number);
            });
        }

        [TestMethod]
        public void ContentPropertyNameAsAttachedPropertyTest()
        {
            ContentPropertyNameTestClass obj = new ContentPropertyNameTestClass { Data = 0 };

            //test if the serializer works as expected
            var generated = SaveToString(obj);
            var expected = @"<ContentPropertyNameTestClass Data=""0"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod, TestDisabled] //TODO, 549050
        public void TypeConverterReturnsStringEmptyWithAttachedProperty()
        {
            StructWrapper sw = new StructWrapper { Value = new Tiger("") };
            StructWrapper.SetA(sw.Value, 100);

            RoundtripObject(sw.Value, t2 =>
            {
                Assert.AreEqual(((Tiger)sw.Value).NickName, ((Tiger)t2).NickName);
            });
        }

        [TestMethod]
        public void DateTimeTest()
        {
            DateTime d1 = DateTime.Now;
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.Now should work");
            });

            d1 = DateTime.MaxValue;
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.MaxValue should work");
            });

            d1 = DateTime.UtcNow;
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.UtcNow should work");
            });

            d1 = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local);
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.UtcNow converted to local should work");
            });

            d1 = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.Now converted to Local should work");
            });

            d1 = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.Now converted to Utc should work");
            });

            d1 = new DateTime(1);
            RoundtripObject<DateTime>(d1, delegate(DateTime dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTime.Now converted to Utc should work");
            });
        }

        [TestMethod]
        public void SerializeType()
        {
            var container = new TypeContainer { Type = typeof(Test.Elements.Element) };
            RoundtripObject<TypeContainer>(container, delegate(TypeContainer container2)
            {
                Assert.IsTrue(container.Type.IsAssignableFrom(container2.Type));
                Assert.IsTrue(container2.Type.IsAssignableFrom(container.Type));
            });

            container.Type = typeof(List<Test.Elements.Element>);
            RoundtripObject<TypeContainer>(container, delegate(TypeContainer container2)
            {
                Assert.IsTrue(container.Type.IsAssignableFrom(container2.Type));
                Assert.IsTrue(container2.Type.IsAssignableFrom(container.Type));
            });
        }


        [TestMethod]
        public void SerializeDateTimeOffset()
        {
            DateTimeOffset d1 = DateTimeOffset.Now;
            RoundtripObject<DateTimeOffset>(d1, delegate(DateTimeOffset dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTimeOffset should work");
            });

            d1 = DateTimeOffset.MaxValue;
            RoundtripObject<DateTimeOffset>(d1, delegate(DateTimeOffset dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTimeOffset should work");
            });

            d1 = DateTimeOffset.UtcNow;
            RoundtripObject<DateTimeOffset>(d1, delegate(DateTimeOffset dt)
            {
                Assert.AreEqual(d1, dt, "Roundtrip of DateTimeOffset should work");
            });
        }

        [TestMethod]
        public void XData()
        {
            var xml = @"<![CDATA[CDATA is preserved.]]>
    <?ProcessingInstructions?>
    TextNodes
    <!-- Comments are preserved -->
    <some xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"">
        <xml isParsedAsXaml=""false"" />
    </some>
    <x:XData xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">Nested once<x:XData>twice</x:XData></x:XData>";
            var ship = new CargoShip();
            var xmlReaderSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Auto };
            using (var reader = XmlReader.Create(new StringReader(xml), xmlReaderSettings))
            {
                ship.Loader.ReadXml(reader);
            }

            var builder = new StringBuilder();
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true,
            };

            using (var writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(builder), xmlWriterSettings), new XamlSchemaContext()))
            {
                XamlServices.Save(writer, ship);
            }

            // TODO correctly detect content property -- see XamlObjectSerializer.Serializer.GetPropertyVisibility?

            var target = @"<CargoShip xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:XData><![CDATA[CDATA is preserved.]]>
    <?ProcessingInstructions?>
    TextNodes
    <!-- Comments are preserved -->
    <some xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"">
        <xml isParsedAsXaml=""false"" />
    </some>
    <x:XData xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">Nested once<x:XData>twice</x:XData></x:XData></x:XData>
</CargoShip>";

            Assert.AreEqual(target, builder.ToString());
        }

        [TestMethod]
        public void XLangTest()
        {
            XmlLangTestClass xmlLangTest = new XmlLangTestClass() { Culture = "en-US" };
            string xaml = XamlServices.Save(xmlLangTest);

            string expected = @"<XmlLangTestClass xml:lang=""en-US"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";

            Assert.AreEqual(expected, xaml);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void InternalSaveWithoutLocalAssembly()
        {
            InternalType s = new InternalType();
            SaveToString(s);
        }

		[TestMethod]
        public void InternalSaveWithLocalAssembly()
        {
            InternalType s = new InternalType();
            s.InternalProp = "Internal";
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlObjectReader r = new XamlObjectReader(s, new XamlObjectReaderSettings { LocalAssembly = typeof(InternalType).Assembly });
                XamlXmlWriter w = new XamlXmlWriter(xw, r.SchemaContext);
                XamlServices.Transform(r, w);
            }

            string expected = @"<InternalType InternalProp=""Internal"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" />";
            Assert.AreEqual(sw.ToString(), expected);
        }

        [TestMethod]
        public void InternalSaveWithLocalAssemblyAndProtectedMemberOnRoot()
        {
            InternalType s = new InternalType();
            s.InternalProp = "Internal";
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlObjectReader r = new XamlObjectReader(s, new XamlObjectReaderSettings { LocalAssembly = typeof(InternalType).Assembly, AllowProtectedMembersOnRoot = true });
                XamlXmlWriter w = new XamlXmlWriter(xw, r.SchemaContext);
                XamlServices.Transform(r, w);
            }

            string expected = @"<InternalType InternalProp=""Internal"" ProtectedProp=""ProtectedPropertyName"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" />";
            Assert.AreEqual(sw.ToString(), expected);
        }

        [TestMethod]
        public void InternalSaveWithLocalAssemblyAndInternalCtor()
        {
            InternalTypeWithInternalCtor s = new InternalTypeWithInternalCtor("Hello");
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlObjectReader r = new XamlObjectReader(s, new XamlObjectReaderSettings { LocalAssembly = typeof(InternalTypeWithInternalCtor).Assembly });
                XamlXmlWriter w = new XamlXmlWriter(xw, r.SchemaContext);
                XamlServices.Transform(r, w);
            }
            string expected =
@"<InternalTypeWithInternalCtor xmlns=""clr-namespace:DrtXaml.Tests;assembly=DrtXaml"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:String>Hello</x:String>
  </x:Arguments>
</InternalTypeWithInternalCtor>";
            Assert.AreEqual(sw.ToString(), expected);
        }
       
        [TestMethod]
        public void WhitespaceSigColTest()
        {
            var container = new WhitespaceSigRunCollectionContainer();

            container.Col.Add(new MyRun { Text = "hel lo " });
            container.Col.Add(new MyRun { Text = "world" });
            container.Col.Add(new MyRun { Text = " mo on " });
            container.Col.Add(new MyRun { Text = "mars" });
            container.Col.Add(new MyRun { Text = " ven ice" });

            string generated = SaveToString(container);
            string expected =
@"<WhitespaceSigRunCollectionContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <WhitespaceSigRunCollectionContainer.Col>hel lo <MyRun>world</MyRun> mo on <MyRun>mars</MyRun> ven ice</WhitespaceSigRunCollectionContainer.Col>
</WhitespaceSigRunCollectionContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void WhitespaceSigColTestWithConsecutiveWhiteSpaces()
        {
            var container = new WhitespaceSigRunCollectionContainer();

            container.Col.Add(new MyRun { Text = "hello" });
            container.Col.Add(new MyRun { Text = "world" });
            container.Col.Add(new MyRun { Text = "mo  on" });
            container.Col.Add(new MyRun { Text = " ma  rs " });
            container.Col.Add(new MyRun { Text = " ven ice" });

            string generated = SaveToString(container);
            string expected =
@"<WhitespaceSigRunCollectionContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <WhitespaceSigRunCollectionContainer.Col>hello<MyRun>world</MyRun><MyRun xml:space=""preserve"">mo  on</MyRun><MyRun xml:space=""preserve""> ma  rs </MyRun> ven ice</WhitespaceSigRunCollectionContainer.Col>
</WhitespaceSigRunCollectionContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void WhitespaceSigColTestWithLeadingSpaceInFirstElem()
        {
            var container = new WhitespaceSigRunCollectionContainer();

            container.Col.Add(new MyRun { Text = " hel lo" });
            container.Col.Add(new MyRun { Text = " world" });
            container.Col.Add(new MyRun { Text = "moon" });
            container.Col.Add(new MyRun { Text = "mars" });
            container.Col.Add(new MyRun { Text = " ven ice" });

            string generated = SaveToString(container);
            string expected =
@"<WhitespaceSigRunCollectionContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <WhitespaceSigRunCollectionContainer.Col>
    <MyRun xml:space=""preserve""> hel lo</MyRun> world<MyRun>moon</MyRun>mars<MyRun xml:space=""preserve""> ven ice</MyRun></WhitespaceSigRunCollectionContainer.Col>
</WhitespaceSigRunCollectionContainer>";
            
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void WhitespaceSigColTestWithTrailingSpaceInLastElement()
        {
            var container = new WhitespaceSigRunCollectionContainer();

            container.Col.Add(new MyRun { Text = "hello" });
            container.Col.Add(new MyRun { Text = "world" });
            container.Col.Add(new MyRun { Text = "moon" });
            container.Col.Add(new MyRun { Text = "mars" });
            container.Col.Add(new MyRun { Text = " ven ice " });

            string generated = SaveToString(container);
            string expected =
@"<WhitespaceSigRunCollectionContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <WhitespaceSigRunCollectionContainer.Col>hello<MyRun>world</MyRun>moon<MyRun>mars</MyRun><MyRun xml:space=""preserve""> ven ice </MyRun></WhitespaceSigRunCollectionContainer.Col>
</WhitespaceSigRunCollectionContainer>";
            
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void InternalTypeWithTCTest()
        {
            ContainerClassForInternalTypeTest cc = new ContainerClassForInternalTypeTest();
            string generated = SaveToString(cc);
            string expected = @"<ContainerClassForInternalTypeTest Prop=""InternalDerivedClassName"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod]
        public void ListOfArrayTypes()
        {
            List<Type> list = new List<Type>()
            {
                typeof(string[]),
                typeof(string[,]),
                typeof(string[,][]),
                typeof(List<string[]>),
                typeof(List<string[,]>),
                typeof(List<string[][]>),
                typeof(Dictionary<string[][], string[,]>),
                typeof(List<string>[]),
                typeof(List<string[]>[]),
                typeof(List<string[]>[][]),
                typeof(List<string[]>[][,]),
                typeof(Dictionary<List<string[]>[], int>),
            };
            RoundtripObject(list, roundTrippedList => Assert.AreEqualOrdered(roundTrippedList, list.ToArray()));
        }

        [TestMethod]
        public void ReadWriteGenericDictContainerTest()
        {
            ReadWriteGenericDictionaryContainer c = new ReadWriteGenericDictionaryContainer();
            c.GenericDict = new Dictionary<int, string>() { { 1, "Foo" } };
            string generated = SaveToString(c);
            string expected = @"<ReadWriteGenericDictionaryContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ReadWriteGenericDictionaryContainer.GenericDict>
    <x:String x:Key=""1"">Foo</x:String>
  </ReadWriteGenericDictionaryContainer.GenericDict>
</ReadWriteGenericDictionaryContainer>";
            Assert.AreEqual(generated, expected);
        }

        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException), typeof(ArgumentException))]
        public void ExceptionWrapping()
        {
            BadGetter bg = new BadGetter();
            XamlServices.Save(bg);
        }

        [TestMethod]
        public void AttributesOnVirtualProperties()
        {
            DerivedWithOverridenProperties dwop = new DerivedWithOverridenProperties("Hello");
            string generated = SaveToString(dwop);
            string expected =
@"<DerivedWithOverridenProperties xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <x:Arguments>
    <x:String>Hello</x:String>
  </x:Arguments>
</DerivedWithOverridenProperties>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void AttachedPropertyReadOnlyCollection()
        {
            object o = new object();
            ReadOnlyAPP.GetStringsList(o).Add("Hello");
            ReadOnlyAPP.GetStringsList(o).Add("Goodbye");
            ReadOnlyAPP.GetStringsDict(o).Add("key1", "Foo");
            ReadOnlyAPP.GetStringsDict(o).Add("key2", "Bar");

            string generated = SaveToString(o);
            string expected = @"<x:Object xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <ReadOnlyAPP.StringsDict>
    <x:String x:Key=""key1"">Foo</x:String>
    <x:String x:Key=""key2"">Bar</x:String>
  </ReadOnlyAPP.StringsDict>
  <ReadOnlyAPP.StringsList>
    <x:String>Hello</x:String>
    <x:String>Goodbye</x:String>
  </ReadOnlyAPP.StringsList>
</x:Object>";
            Assert.AreEqual(expected, generated);

            RoundtripObject(o, o1 =>
            {
                IList<string> list = ReadOnlyAPP.GetStringsList(o1);
                IDictionary<string, string> dict = ReadOnlyAPP.GetStringsDict(o1);

                Assert.IsNotNull(list);
                Assert.AreEqual(2, list.Count);
                Assert.AreEqual("Hello", list[0]);
                Assert.AreEqual("Goodbye", list[1]);

                Assert.IsNotNull(dict);
                Assert.AreEqual(2, dict.Count);
                Assert.AreEqual("Foo", dict["key1"]);
                Assert.AreEqual("Bar", dict["key2"]);
            });
        }

        //        [TestMethod] 
        //        public void ReferenceInMarkupExtension()
        //        {
        //            IList<object> things = new List<object>();
        //            MERefTestExtension be = new MERefTestExtension();
        //            MERefTest b = (MERefTest)be.ProvideValue(null);
        //            be.Value = b;
        //            things.Add(b);
        //            things.Add(be);

        //            var expected = @"<List x:TypeArguments=""p:Object"" Capacity=""4"" xmlns=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"" xmlns:tx=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:x2=""http://schemas.microsoft.com/netfx/2008/xaml"">
        //  <tx:MERefTest Reference=""{x2:Reference Name=&quot;__ReferenceID0&quot;}"" x:Name=""__ReferenceID0"" />
        //  <tx:MERefTestExtension Value=""{x2:Reference Name=&quot;__ReferenceID0&quot;}"" />
        //</List>";
        //            Assert.AreEqual(expected, SaveToString(things));
        //        }

        //[TestMethod]
        //public void CollectionConverterTest()
        //{
        //    Zoo zoo = new Zoo();
        //    Animal tiger = new Animal();
        //    tiger.Name = "Tiger";
        //    tiger.Number = 2;
        //    Animal monkey = new Animal();
        //    monkey.Name = "Monkey";
        //    monkey.Number = 3;
        //    zoo.Animals.Add(tiger);
        //    zoo.Animals.Add(monkey);

        //    RoundtripObject(zoo, o =>
        //    {
        //        Assert.AreEqual(2, o.Animals.Count);
        //    });
        //}

        /*
        [TestMethod]
        public void ContentPropertyNotOnBase()
        {
            var obj = new ContentPropertyTestBase { ATest = 123 };

            var expected = @"<ContentPropertyTestBase xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"">
  <ContentPropertyTestBase.ATest>
    <p:Int32>123</p:Int32>
  </ContentPropertyTestBase.ATest>
</ContentPropertyTestBase>";

            Assert.AreEqual(expected, SaveToString(obj));
        }
        [TestMethod]
        public void ContentPropertyOnDerived()
        {
            var obj = new ContentPropertyTestDerived { ATest = 123 };

            var expected = @"<ContentPropertyTestDerived xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"">
  <p:Int32>123</p:Int32>
</ContentPropertyTestDerived>";
            Assert.AreEqual(expected, SaveToString(obj));
        }
        [TestMethod]
        public void CrazyGenerics()
        {
            GenericContainerType2<sbyte, TimeSpan, string> instance1 = new GenericContainerType2<sbyte, TimeSpan, string>
                {
                    Infos = new GenericType2<sbyte[], TimeSpan[][], string[][][]>
                    {
                        Info1 = new sbyte[] { sbyte.MinValue, sbyte.MaxValue, 0 },
                        Info2 = new TimeSpan[][] { new TimeSpan[] { }, new TimeSpan[] { TimeSpan.MaxValue, TimeSpan.MinValue } },
                        Info3 = new string[][][] { new string[][] { }, new string[][] { new string[] { } }, new string[][] { new string[] { "a", "b" } } },
                    }
                };

            var iTypeRef = XamlSchemaTypeResolver.Default.GetTypeReference(instance1);
            Assert.IsNotNull(iTypeRef);
            var iType = XamlSchemaTypeResolver.Default.Resolve(iTypeRef);
            Assert.IsNotNull(iType);
            Assert.IsNotNull(iType.GetClrType());
            Assert.AreEqual(instance1.GetType(), iType.GetClrType());

            var i2TypeRef = XamlSchemaTypeResolver.Default.GetTypeReference(instance1.Infos);
            Assert.IsNotNull(i2TypeRef);
            var i2Type = XamlSchemaTypeResolver.Default.Resolve(i2TypeRef);
            Assert.IsNotNull(i2Type);
            Assert.IsNotNull(i2Type.GetClrType());
            Assert.AreEqual(instance1.Infos.GetType(), i2Type.GetClrType());

            RoundtripObject(instance1, i =>
            {
                Assert.IsNotNull(i);
            });
        }

        [TestMethod]
        public void Enum()
        {
            var person = new PersonWithEnumProperty
                {
                    Name = "Gudge",
                    Age = AgeEnum.Venerable
                };

            var xaml = XamlServices.Save(person);
            var expected = @"<PersonWithEnumProperty Age=""Venerable"" Name=""Gudge"" xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" />";
            Assert.AreEqual(expected, xaml);
            person = (PersonWithEnumProperty)XamlServices.Parse(xaml);
            Assert.AreEqual(person.Age, AgeEnum.Venerable);
            Assert.AreEqual(person.Name, "Gudge");
        }
        
        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void NestedClassFail()
        {
            Outer.Inner i = new Outer.Inner { B = "Something" };
            var sw = new StringWriter();
            XamlServices.Save(sw, i);
        }
        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void NestedStructFail()
        {
            var i = new NestedStruct.InnerStruct();
            XamlServices.Save(new StringWriter(), i);
        }
        [TestMethod, TestExpectedException(typeof(XamlObjectReaderException))]
        public void NoDefaultCtor()
        {
            NoDefaultCtor n = new NoDefaultCtor("a");
            var sw = new StringWriter();
            XamlServices.Save(sw, n);
        }
        [TestMethod]
        [TestExpectedException(typeof(ArgumentException))]
        public void NonStaticMethodNoCtor()
        {
            PointWithNonStaticMethod obj = PointWithNonStaticMethod.CreateSecret(1, 2);
            NonStaticMethodNoCtor_Test(obj);
            RoundtripObject(obj, NonStaticMethodNoCtor_Test);
        }
        void NonStaticMethodNoCtor_Test(object obj)
        {
            PointWithNonStaticMethod pt = (PointWithNonStaticMethod)obj;
            Assert.AreEqual(1, pt.X);
            Assert.AreEqual(2, pt.Y);
        }
        [TestMethod]
        public void PatternBasedSequence()
        {
            PatternSequence pbs = new PatternSequence();
            pbs.Add(new FooNamed { Name = "Josh" });
            pbs.Add(12);
            pbs.Add(new int[] { 1, 2, 3 });

            var expected = @"<PatternSequence xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <FooNamed Name=""Josh"" />
  <p:Int32>12</p:Int32>
  <x:Array Type=""p:Int32"">
    <p:Int32>1</p:Int32>
    <p:Int32>2</p:Int32>
    <p:Int32>3</p:Int32>
  </x:Array>
</PatternSequence>";

            Assert.AreEqual(expected, SaveToString(pbs));
        }

        [TestMethod]
        [TestExpectedException(typeof(System.Xaml.XamlParseException))]
        public void RuntimeNameNotStringType()
        {
            FooBad obj = new FooBad();
            RuntimeNameNotStringType_Test(obj);
            RoundtripObject(obj, RuntimeNameNotStringType_Test);
        }
        void RuntimeNameNotStringType_Test(object obj)
        {
            FooBad foo = (FooBad)obj;
        }

        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void SaveStreamFailNull()
        {
            Stream s = null;
            XamlObjectSerializer os = new XamlObjectSerializer();
            Foo f = new Foo();
            os.Save(s, f);
        }
        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void SaveTextWriterFailNull()
        {
            TextWriter tw = null;
            XamlObjectSerializer os = new XamlObjectSerializer();
            Foo f = new Foo();
            os.Save(tw, f);
        }
        string SaveToString(object o)
        {
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xw, o);
            }
            return sw.ToString();
        }
        [TestMethod]
        [TestExpectedException(typeof(ArgumentNullException))]
        public void SaveXmlWriterFailNull()
        {
            XmlWriter xw = null;
            XamlObjectSerializer os = new XamlObjectSerializer();
            Foo f = new Foo();
            os.Save(xw, f);
        }
        [TestMethod]
        public void SimplifiedNamespaces()
        {
            SchemaType xt = new SchemaType();
            xt.Name = "Hello";

            var sb = new StringBuilder();
            var xamlSettings = new XamlWriterSettings { CloseOutput = true };
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            using (var writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(sb), xmlSettings), xamlSettings))
            {
                XamlServices.Save(writer, xt);
            }

            var expected = @"<p:SchemaType BaseType=""p:Object"" Name=""Hello"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"" />";
            Assert.AreEqual(expected, sb.ToString());
        }

        [TestMethod]
        public void TypeConverterOnPropertyWithPublicNestedType()
        {
            UsesNested nestedContainer = new UsesNested();
            nestedContainer.MyNestedType = new ParentOfNested.Nested() { MyString = "string on nested type" };

            RoundtripObject(nestedContainer, roundTripped =>
                {
                    Assert.AreEqual(((ParentOfNested.Nested)nestedContainer.MyNestedType).MyString, ((ParentOfNested.Nested)roundTripped.MyNestedType).MyString);
                });
        }
        [TestMethod]
        public void TypeConverterOnPropertyWithPrivateNestedType()
        {
            UsesNestedPrivate nestedContainer = new UsesNestedPrivate();

            RoundtripObject(nestedContainer, roundTripped =>
            {
                Assert.AreEqual(nestedContainer.MyNestedType.MyString, roundTripped.MyNestedType.MyString);
            });
        }
        [TestMethod]
        public void TypeConverterReturnsStringEmpty()
        {
            Tiger t = new Tiger("");

            RoundtripObject(t, t2 =>
            {
                Assert.AreEqual(t.NickName, t2.NickName);
            });
        }

        [TestMethod]
        public void Unparsed()
        {
            var scwup = new SimpleClassWithUnparsedMembers
                {
                    SingleValueUnparsed = GetXamlReader()
                };

            var xaml = XamlServices.Save(scwup);
            object obj = XamlServices.Parse(xaml);
            Assert.IsNotNull(obj);
            scwup = obj as SimpleClassWithUnparsedMembers;
            Assert.IsNotNull(scwup);
            obj = XamlServices.Load(scwup.SingleValueUnparsed);
            Assert.IsNotNull(obj);
            Simple s = obj as Simple;
            Assert.IsNotNull(s);
        }
        System.Xaml.XamlReader GetXamlReader()
        {
            Simple s = new Simple { Prop = "Hello World" };
            return new XamlXmlReader(new StringReader(XamlServices.Save(s)));
        }
        [TestMethod]
        public void WhitespaceInStringsAttribute()
        {
            StringProps sp = new StringProps();
            sp.A = "Hello World";
            sp.B = "   Goodbye,";
            sp.C = "Cruel World    ";

            var sb = new StringBuilder();
            var xamlSettings = new XamlWriterSettings { CloseOutput = true};
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            using (var writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(sb), xmlSettings), xamlSettings))
            {
                XamlServices.Save(writer, sp);
            }
            var expected = @"<StringProps A=""Hello World"" B=""   Goodbye,"" C=""Cruel World    "" xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" />";
            Assert.AreEqual(expected, sb.ToString());
        }
        [TestMethod]
        public void WhitespaceInStringsElement()
        {
            StringProps sp = new StringProps();
            sp.A = "Hello World";
            sp.B = "   Goodbye,";
            sp.C = "Cruel World    ";

            var sb = new StringBuilder();
            var xamlSettings = new XamlWriterSettings { CloseOutput = true };
            var xmlSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            using (var writer = new System.Xaml.XamlXmlWriter(XmlWriter.Create(new StringWriter(sb), xmlSettings), xamlSettings))
            {
                XamlServices.Save(writer, sp);
            }
            var expected = @"<StringProps xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"">
  <StringProps.A>Hello World</StringProps.A>
  <StringProps.B xml:space=""preserve"">   Goodbye,</StringProps.B>
  <StringProps.C xml:space=""preserve"">Cruel World    </StringProps.C>
</StringProps>";
            Assert.AreEqual(expected, sb.ToString());
        }
        [TestMethod]
        public void XmlDocument()
        {
            ClassType5 data = new ClassType5();

            string xmlDoc;
            using (MemoryStream stream = new MemoryStream())
            {
                XamlServices.Save(stream, data);
                stream.Position = 0;

                XmlDocument objectDoc = new XmlDocument();
                objectDoc.Load(stream);

                xmlDoc = String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><Root>{0}</Root>", objectDoc.DocumentElement.OuterXml);
            }

            using (XmlReader reader = XmlReader.Create(new StringReader(xmlDoc)))
            {
                reader.MoveToContent();
                reader.ReadStartElement("Root");

                ClassType5 obj = (ClassType5)XamlServices.Load(reader.ReadSubtree());
                Assert.IsNotNull(obj);
                Assert.IsNotNull(obj.Field1);
                Assert.AreEqual(obj.Field1.Category, data.Field1.Category);
            }
        }
        [TestMethod]
        public void XDataNamespaceHandling()
        {
            TypeContaingingIXmlSerializableProperty t = new TypeContaingingIXmlSerializableProperty { data = "Some string" };

            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xw, t);
            }

            var xaml = @"<TypeContaingingIXmlSerializableProperty xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <TypeContaingingIXmlSerializableProperty.XmlProperty>
    <x:XData>
      <SimpleIXmlSerializable xmlns=""clr-namespace:DrtXaml.Tests;assembly=CDF.CIT.Scenarios.Xaml.XamlTest"">Some string</SimpleIXmlSerializable>
    </x:XData>
  </TypeContaingingIXmlSerializableProperty.XmlProperty>
</TypeContaingingIXmlSerializableProperty>";

            Assert.AreEqual(xaml, sw.ToString());
        }

        */
        string SaveToString(object o)
        {
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xw, o);
            }
            return sw.ToString();
        }
     
        void RoundtripObject<T>(T o, Action<T> test)
        {
            string generated = SaveToString(o);
            object o2 = XamlServices.Parse(generated);
            test((T)o2);
        }

        void RoundtripObject_String(object o, Action<object> test)
        {
            StringBuilder sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                XamlServices.Save(sw, o);
            }

            using (var sr = new StringReader(sb.ToString()))
            {
                using (var reader = XmlReader.Create(sr))
                {
                    object o2 = XamlServices.Load(reader);
                    test(o2);
                }
            }
        }
    }

    internal class InternalType
    {
        string protectedProp = "ProtectedPropertyName";
        protected string ProtectedProp
        {
            get
            {
                return protectedProp;
            }
            set
            {
                protectedProp = value;
            }
        }

        internal string InternalProp { get; set; }
    }

    internal class InternalTypeWithInternalCtor
    {
        [ConstructorArgument("foo")]
        public string Foo { get; private set; }

        internal InternalTypeWithInternalCtor(string foo)
        {
            Foo = foo;
        }
    }
}
