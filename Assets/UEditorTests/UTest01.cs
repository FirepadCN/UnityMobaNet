using NUnit.Framework;
using ProtoMsg;

namespace Tests
{
    public class UTest01
    {
        // A Test behaves as an ordinary method

        [Test]
        public void UTestInitCar()
        {
            ICar car = new Car();
            IEngine engine = new CX001Engine();
            car.Init(engine);
            Assert.AreEqual((car as Car).Engine,engine);
        }

        [Test]
        public void TestProtobf()
        {
            UserInfo user=new UserInfo(){Account="aaa",ID =1,Password = "sss"};
            byte[] userBytes= ProtobufHelper.ToBytes(user);
            var userInfo = ProtobufHelper.FromBytes<UserInfo>(userBytes);
            Assert.IsTrue((user.Password.Equals(userInfo.Password)&&user.ID.Equals(userInfo.ID)&&user.Account.Equals(userInfo.Account)));
            
        }
    }
}
