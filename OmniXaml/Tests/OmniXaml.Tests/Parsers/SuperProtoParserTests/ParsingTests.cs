﻿namespace OmniXaml.Tests.Parsers.SuperProtoParserTests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Classes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OmniXaml.Parsers;
    using OmniXaml.Parsers.ProtoParser;
    using Xaml.Tests.Resources;

    [TestClass]
    public class ParsingTests : GivenAWiringContext
    {
        private readonly ProtoInstructionBuilder builder;
        private IParser<Stream, IEnumerable<ProtoXamlInstruction>> sut;

        public ParsingTests()
        {
            builder = new ProtoInstructionBuilder(WiringContext.TypeContext, WiringContext.FeatureProvider);
        }

        [TestInitialize]
        public void Initialize()
        {
            sut = new XamlProtoInstructionParser(WiringContext);
        }

        [TestMethod]
        public void SingleCollapsed()
        {
            var actualNodes = sut.Parse(ProtoInputs.SingleCollapsed).ToList();

            ICollection expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
               builder.EmptyElement<DummyClass>(RootNs),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void SingleOpenAndClose()
        {
            var actualNodes = sut.Parse(ProtoInputs.SingleOpenAndClose).ToList();

            ICollection expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void ElementWithChild()
        {
            var actualNodes = sut.Parse(ProtoInputs.ElementWithChild).ToList();

            ICollection expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Child, RootNs),
                builder.EmptyElement(typeof(ChildClass), RootNs),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void SingleCollapsedWithNs()
        {
            var actualNodes = sut.Parse(ProtoInputs.SingleCollapsedWithNs).ToList();

            ICollection expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.EmptyElement<DummyClass>(RootNs),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void ElementWith2NsDeclarations()
        {
            var actualNodes = sut.Parse(ProtoInputs.ElementWith2NsDeclarations).ToList();

            var expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NamespacePrefixDeclaration("a", "another"),
                builder.EmptyElement<DummyClass>(RootNs),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void SingleOpenWithNs()
        {
            var actualStates = sut.Parse(ProtoInputs.SingleOpenAndCloseWithNs).ToList();

            var expectedStates = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass),  RootNs),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedStates, actualStates);
        }

        [TestMethod]
        [ExpectedException(typeof(XamlParseException))]
        public void PropertyTagOpen()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            sut.Parse(ProtoInputs.PropertyTagOpen).ToList();
        }

        [TestMethod]
        public void InstanceWithStringPropertyAndNsDeclaration()
        {
            var actualNodes = sut.Parse(Dummy.StringProperty).ToList();

            var expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.Attribute<DummyClass>(d => d.SampleProperty, "Property!", RootNs),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void KeyDirective()
        {
            var actualNodes = sut.Parse(Dummy.KeyDirective).ToList();

            var expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NamespacePrefixDeclaration("x", "http://schemas.microsoft.com/winfx/2006/xaml"),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Resources, RootNs),
                builder.EmptyElement(typeof(ChildClass), RootNs),
                builder.Key("SomeKey"),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void AttachedProperty()
        {
            var actualNodes = sut.Parse(Dummy.WithAttachableProperty).ToList();

            var prefix = "root";

            var expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration("", prefix),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.AttachableProperty<Container>("Property", "Value", RootNs),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void ThreeLevelsOfNesting()
        {
            var actualNodes = sut.Parse(Dummy.ThreeLevelsOfNesting).ToList();

            ICollection expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof (DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Child, RootNs),
                builder.NonEmptyElement(typeof (ChildClass), RootNs),
                builder.NonEmptyPropertyElement<ChildClass>(d => d.Child, RootNs),
                builder.EmptyElement(typeof (ChildClass), RootNs),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void FourLevelsOfNesting()
        {
            var actualNodes = sut.Parse(Dummy.FourLevelsOfNesting).ToList();

            ICollection expectedInstructions = new Collection<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Child, RootNs),
                builder.NonEmptyElement(typeof(ChildClass), RootNs),
                builder.NonEmptyPropertyElement<ChildClass>(d => d.Child, RootNs),
                builder.NonEmptyElement(typeof(ChildClass), RootNs),
                builder.NonEmptyPropertyElement<ChildClass>(d => d.Child, RootNs),
                builder.EmptyElement(typeof(ChildClass), RootNs),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void ChildCollection()
        {
            var actualNodes = sut.Parse(Dummy.ChildCollection).ToList();
            var expectedInstructions = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Items, RootNs),
                builder.EmptyElement(typeof(Item), RootNs),
                builder.Text(),
                builder.EmptyElement(typeof(Item), RootNs),
                builder.Text(),
                builder.EmptyElement(typeof(Item), RootNs),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void ContentPropertyForCollectionOneElement()
        {
            var actualNodes = sut.Parse(Dummy.ContentPropertyForCollectionOneElement).ToList();
            var expectedInstructions = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.EmptyElement(typeof(Item), RootNs),
                builder.Text(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void CollapsedTag()
        {
            var actualNodes = sut.Parse(Dummy.CollapsedTag).ToList();
            var expectedInstructions = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.EmptyElement(typeof(DummyClass), RootNs),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void TwoNestedPropertiesEmpty()
        {
            var actualNodes = sut.Parse(Dummy.TwoNestedPropertiesEmpty).ToList();
            var expectedInstructions = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Items, RootNs),
                builder.EndTag(),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Child, RootNs),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void TwoNestedProperties()
        {
            var actualNodes = sut.Parse(Dummy.TwoNestedProperties).ToList();
            var expectedInstructions = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof(DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Items, RootNs),
                builder.EmptyElement<Item>(RootNs),
                builder.Attribute<Item>(i => i.Title, "Main1", RootNs),
                builder.Text(),
                builder.EmptyElement<Item>(RootNs),
                builder.Attribute<Item>(i => i.Title, "Main2", RootNs),
                builder.Text(),
                builder.EndTag(),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.Child, RootNs),
                builder.NonEmptyElement(typeof(ChildClass), RootNs),
                builder.EndTag(),
                builder.Text(),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void ExpandedStringProperty()
        {
            var actualNodes = sut.Parse(Dummy.InnerContent).ToList();

            var expectedInstructions = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(RootNs),
                builder.NonEmptyElement(typeof (DummyClass), RootNs),
                builder.NonEmptyPropertyElement<DummyClass>(d => d.SampleProperty, RootNs),
                builder.Text("Property!"),
                builder.EndTag(),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedInstructions, actualNodes);
        }

        [TestMethod]
        public void String()
        {
            var actualStates = sut.Parse(Dummy.String).ToList();

            var sysNs = new NamespaceDeclaration("clr-namespace:System;assembly=mscorlib", "sys");
            var expectedStates = new List<ProtoXamlInstruction>
            {
                builder.NamespacePrefixDeclaration(sysNs),
                builder.NonEmptyElement(typeof (string), sysNs),
                builder.Text("Text"),
                builder.EndTag(),
            };

            CollectionAssert.AreEqual(expectedStates, actualStates);
        }
    }
}
