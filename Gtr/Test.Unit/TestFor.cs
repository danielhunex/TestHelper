using Moq;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Gtr.Test.Unit
{
    public class TestFor<T> where T : class
    {
        private ConcurrentDictionary<Type, Mock> _mocks = new ConcurrentDictionary<Type, Mock>();
        private T _target;

        public virtual T Target
        {
            get
            {
                if (_target == null)
                {
                    var constructorInfo = typeof(T).GetConstructors();

                    var ctrManyPrmtrs = constructorInfo.Where(ctr => !ctr.IsPrivate)
                        .OrderByDescending(info => info.GetParameters().Count())
                        .First();

                    var mockParams = new List<object>();
                    foreach (var param in ctrManyPrmtrs.GetParameters())
                    {
                        if (_mocks.ContainsKey(param.ParameterType))
                        {
                            mockParams.Add(_mocks[param.ParameterType].Object);
                        }
                        else
                        {
                            var mock = CreateMock(param.ParameterType);
                            mockParams.Add(mock.Object);
                        }
                    }

                    _target = ctrManyPrmtrs.Invoke(mockParams.ToArray()) as T;

                    TargetSetup();
                }
                return _target;
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            _mocks = new ConcurrentDictionary<Type, Mock>();
            _target = null;
        }

        public Mock<TD> The<TD>() where TD : class
        {
            if (_mocks.ContainsKey(typeof(TD)))
            {
                return _mocks[typeof(TD)] as Mock<TD>;
            }
            return CreateMock(typeof(TD)) as Mock<TD>;
        }

        protected virtual void TargetSetup()
        {
        }

        private Mock CreateMock(Type type)
        {
            var genericMockedType = typeof(Mock<>).MakeGenericType(type);
            var mock = Activator.CreateInstance(genericMockedType) as Mock;
            _mocks.GetOrAdd(type, mock);
            return mock;
        }
    }
}
