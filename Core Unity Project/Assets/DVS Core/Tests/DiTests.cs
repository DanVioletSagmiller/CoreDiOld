using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Dvs.Core.IoC;
using Dvs.Core.IoC.Exceptions;
using System;

public class DiTests
{
    public class Classes
    {
        public interface INotConfiguredInDi { }
        public class ClassNotConfiguredInDi : INotConfiguredInDi { }

        public interface ITwoTypes { }
        public class TwoTypesA : ITwoTypes { }
        public class TwoTypesB : ITwoTypes { }

        public class TestEvent
        {
            public int TestValue;
        }
    }

    [Test]
    public void Get_WhenInterfaceIsNotSetupInDi_WillThrowNoBindingsFoundException()
    {
        Assert.Throws(typeof(NoBindingsFoundException),
            () =>
            {
                var actual = Di.Get<Classes.INotConfiguredInDi>();
            });
    }

    [Test]
    public void Get_BoundAsWithAType_WillReturnThatType()
    {
        // Arrange
        Di.Bind<Classes.ITwoTypes>().As<Classes.TwoTypesA>();

        // Act
        var actual = Di.Get<Classes.ITwoTypes>();
        Debug.Log(actual);

        // Assert
        Assert.AreSame(typeof(Classes.TwoTypesA), actual.GetType(), "ITwoTypes was bound as TwoTypesA, but did not return as that type");
    }

    [Test]
    public void Get_WithSecondTypeBound_WillReturnSecondType()
    {
        // Arrange
        Di.Bind<Classes.ITwoTypes>().As<Classes.TwoTypesA>();
        Di.Bind<Classes.ITwoTypes>().As<Classes.TwoTypesB>();
        
        // Act
        var actual = Di.Get<Classes.ITwoTypes>();
        Debug.Log(actual);

        // Assert
        Assert.AreSame(typeof(Classes.TwoTypesB), actual.GetType(), "ITwoTypes was bound as TwoTypesB, but did not return as that type");
    }

    [Test]
    public void TriggerEvent_WithNothingListening_ThrowsNoException()
    {
        Di.TriggerEvent(new Classes.TestEvent());
    }

    [Test]
    public void TriggerEvent_WithListener_WillTriggerThatListener()
    {
        // Arrange
        var insert = new Classes.TestEvent();
        insert.TestValue = UnityEngine.Random.Range(1, 1000000);
        int actual = -1;
        Debug.Log("call setup starting");
        var listener = new Action<Classes.TestEvent>((x) =>
        {
            Debug.Log("called");
            actual = x.TestValue;
        });

        Di.AttachEventListener<Classes.TestEvent>(listener);

        // Act
        Di.TriggerEvent(insert);

        // Assert
        Assert.AreEqual(insert.TestValue, actual);
    }

    [Test]
    public void TriggerEvent_WithListenerRemoved_WillNotTriggerThatListener()
    {
        // Arrange
        var insert = new Classes.TestEvent();
        insert.TestValue = UnityEngine.Random.Range(1, 1000000);
        int actual = -1;
        Debug.Log("call setup starting");
        var listener = new Action<Classes.TestEvent>((x) =>
        {
            Debug.Log("called");
            actual = x.TestValue;
        });

        Di.AttachEventListener(listener);
        Di.AttachEventListener(listener, false);

        // Act
        Di.TriggerEvent(insert);

        // Assert
        Assert.AreEqual(-1, actual);
    }

    [SetUp]
    public void DiTestsSetup()
    {
        Di.Clear();
        Di.ResetDefaults();
    }
}
