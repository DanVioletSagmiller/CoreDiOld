using System;
using System.Collections;
using System.Collections.Generic;
using Dvs.Core.IoC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dvs.Core.IoC.Tests
{
    public class TestDiFactory
    {
        /// <summary>
        /// Clears all previous DI configurations
        /// </summary>
        public void Init()
        {
            Di.Accessor.FullReset();
        }

        [Test]
        public void Get_WhenCalledWithNoSetup_ReturnsDefaultConstructionOfType()
        {
            // Arrange
            Init();
            SimpleObject so = null;

            // Act
            so = Di<SimpleObject>.Factory.Get();

            // Assert
            Assert.AreEqual(SimpleObject.TextDefault, so.Text);
        }

        [Test]
        public void SetConstructor_WhenBoundGenericly_WillReturnObject()
        {
            // Arrange
            Init();
            ISimpleObject actual = null;

            // Act
            Di<ISimpleObject>.Factory.Bind<SimpleObject>();
            actual = Di<ISimpleObject>.Factory.Get();

            // Assert
            Assert.AreEqual(SimpleObject.TextDefault, actual.Text);
        }

        [Test]
        public void SetConstructor_WhenBoundByType_WillReturnObject()
        {
            // Arrange
            Init();
            ISimpleObject actual = null;

            // Act
            Di<ISimpleObject>.Factory.Bind(typeof(SimpleObject));
            actual = Di<ISimpleObject>.Factory.Get();

            // Assert
            Assert.AreEqual(SimpleObject.TextDefault, actual.Text);
        }

        [Test]
        public void Get_WhenCalledMultipleTimes_ReturnsDifferentInstances()
        {
            // Arrange
            Init();
            SimpleObject so = null;
            SimpleObject so2 = null;

            // Act
            so = Di<SimpleObject>.Factory.Get();
            so2 = Di<SimpleObject>.Factory.Get();

            // Assert
            Assert.AreNotSame(so, so2, "The factory returned the same object multiple times.");
        }

        [Test]
        public void UsePool_WhenSet_WillAllowReturnedObjectsToBeReused()
        {
            // Arrange
            Init();
            SimpleObject so = null;
            SimpleObject so2 = null;

            // Act
            Di<SimpleObject>.Factory.UsePool = true;
            so = Di<SimpleObject>.Factory.Get();
            Di<SimpleObject>.Factory.ReturnToPool(so);
            so2 = Di<SimpleObject>.Factory.Get();

            // Assert
            Assert.AreSame(
                so, 
                so2, 
                "The factory created 1 object, which was returned to the pool, but chose to generate a new one instead of gathering from the pool");
        }

        [Test]
        public void SetPoolCleanup_WillBeCalledWhenReturningObjectToPool()
        {
            // Arrange
            Init();
            SimpleObject so = null;
            var expected = "asdfASDf";
            var actual = "";
            Di<SimpleObject>.Factory.UsePool = true;
            Di<SimpleObject>.Factory.SetPoolCleanup((x) =>
            {
                actual = expected;
                return x;
            });


            // Act

            so = Di<SimpleObject>.Factory.Get();
            Di<SimpleObject>.Factory.ReturnToPool(so);

            // Assert
            Assert.AreEqual(
                expected,
                actual,
                "The method handed to SetPoolCleanup was not executed when returning an object to the pool");
        }

        [Test]
        public void SetPoolRestore_WithUsePoolTrue_WillBeCalledWhenGettingAnObject()
        {
            // Arrange
            Init();
            SimpleObject so = null;
            var expected = "asdfASDf";
            var actual = "";
            Di<SimpleObject>.Factory.UsePool = true;
            Di<SimpleObject>.Factory.SetPoolRestore((x) =>
            {
                actual = expected;
                return x;
            });


            // Act
            so = Di<SimpleObject>.Factory.Get();

            // Assert
            Assert.AreEqual(
                expected,
                actual,
                "The method handed to SetPoolRestor was not executed when getting an object from the pool");
        }

        [Test]
        public void SetPoolRestore_WithUsePoolFalse_WillNotBeCalledWhenGettingAnObject()
        {
            // Arrange
            Init();
            SimpleObject so = null;
            var expected = "";

            var actual = "";
            Di<SimpleObject>.Factory.SetPoolRestore((x) =>
            {
                actual = "SetPoolRestore_WithUsePoolFalse_WillNotBeCalledWhenGettingAnObject";
                return x;
            });


            // Act
            so = Di<SimpleObject>.Factory.Get();

            // Assert
            Assert.AreEqual(
                expected,
                actual,
                "The method handed to SetPoolRestore was executed whennot using the pool");
        }

        [Test]
        public void Get_WhenRequestingAnInterfaceWithNoSetup_ThrowsAFactoryNotConfiguredException()
        {
            // Arrange
            Init();
            ISimpleObject so = null;
            Exception thrownException = null;

            // Act
            try
            {
                so = Di<ISimpleObject>.Factory.Get();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            // Assert
            Assert.IsInstanceOf(typeof(DiNotConfiguredException), thrownException);
        }

        [Test]
        public void SetConstructor_WhenGivenAFunctionReturningTheObject_WillAllowGetCallsToReturnTheObject()
        {
            // Arrange
            Init();
            string expectedText = "This should not be the default text";
            ISimpleObject actual = null;

            // Act
            Di<ISimpleObject>.Factory.SetConstructor(() => new SimpleObject() { Text = expectedText });
            actual = Di<ISimpleObject>.Factory.Get();

            // Assert
            Assert.AreEqual(expectedText, actual.Text);
        }

        [Test]
        public void DiAccessorFullReset_WillEraseExistingConfigurations()
        {
            // Arrange
            Init();
            ISimpleObject so = null;
            Exception thrownException = null;

            // Act
            try
            {
                Di<ISimpleObject>.Factory.SetConstructor(() => new SimpleObject());
                Di.Accessor.FullReset();
                so = Di<ISimpleObject>.Factory.Get();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            // Assert
            Assert.IsNotNull(
                thrownException, 
                "A constructor, was retained after attempting a full reset");

            var thrownExceptionIsNotExpectedType = !
                (thrownException is DiNotConfiguredException);
            if (thrownExceptionIsNotExpectedType)
            {
                Assert.Inconclusive("Failed for unexpected reason: " + thrownException.InnerException);
            }
        }
    }
}
