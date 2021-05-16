using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestCarRun
    {
        [UnityTest]
        public IEnumerator TestRun100()
        {
            Car car = new Car();
            IEngine engine = new CX001Engine();
            car.Init(engine);
            car.Speed = 100;
            var state=car.Run();
            Assert.AreEqual(state,"正常行驶！");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestRun10()
        {
            var go=new GameObject();
            Car car = go.AddComponent<Car>();
            IEngine engine = new CX001Engine();
            car.Init(engine);
            car.Speed = -1;
            var state=car.Run();
            Assert.AreEqual(state,"速度异常！");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestRun300()
        {
            Car car = new Car();
            IEngine engine = new CX001Engine();
            car.Init(engine);
            car.Speed = 300;
            var state=car.Run();
            Assert.AreEqual(state,"速度异常！");
            yield return null;
        }
    }
}
