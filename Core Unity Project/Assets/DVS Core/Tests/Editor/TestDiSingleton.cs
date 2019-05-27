using System;
using System.Collections;
using System.Collections.Generic;
using Dvs.Core.IoC;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dvs.Core.IoC.Tests
{
    public class TestDiSingleton
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
            SingletonObject so = null;

            // Act
            so = Di<SingletonObject>.Singleton.Get();

            // Assert
            Assert.AreEqual(SingletonObject.TextDefault, so.Text);
        }

        [Test]
        public void Get_WhenRequestingAnInterfaceWithNoSetup_ThrowsAFactoryNotConfiguredException()
        {
            // Arrange
            Init();
            ISingletonObject so = null;
            Exception thrownException = null;
            //so = Di<ISingletonObject>.Singleton.Get();
            // Act
            try
            {
                so = Di<ISingletonObject>.Singleton.Get();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            // Assert
            Assert.IsInstanceOf(typeof(DiNotConfiguredException), thrownException);
        }

        [Test]
        public void Get_WhenCalledMultipleTimes_ReturnsTheSameObject()
        {
            // Arrange
            Init();
            SingletonObject so = null;
            SingletonObject so2 = null;

            // Act
            so = Di<SingletonObject>.Singleton.Get();
            so2 = Di<SingletonObject>.Singleton.Get();

            // Assert
            Assert.AreSame(
                so,
                so2,
                "Singletons should always return the same object for the same type request, but did not.");
        }
    }
}
