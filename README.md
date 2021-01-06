# Unity Client Framework
[![Download](https://img.shields.io/badge/download-v2.1.2-blue)](https://github.com/nextwingames/unity-client/releases/download/2.1.2/NextwinUnityClient2.1.2.unitypackage)
[![license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/nextwingames/unity-client/blob/main/LICENSE)

유니티 게임 클라이언트 개발 프레임워크입니다. 다양한 기능을 적은 코딩으로 쉽게 구현할 수 있습니다. C# 서버와 통신 시 직렬화를 위해 [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp.git)을 사용하였으며 기타 유틸을 사용하기 위해 [Nextwin C# Library](https://github.com/nextwingames/csharp-lib.git)를 참조합니다. 또한 C#의 Dictionary를 유니티 인스펙터창에서 수정할 수 있도록 [Serializable Dictionary Asset](https://assetstore.unity.com/packages/tools/integration/serializabledictionary-90477)을 포함합니다. 

이 프레임워크를 사용하기 위해 상단의 download 버튼을 클릭하여 유니티 패키지를 다운받은 후, 유니티 프로젝트에 import 하세요.

## Contents
- [MessagePack](#messagepack)
- [Nextwin.Client.Game](#nextwinclientgame)
- [Nextwin.Client.Protocol](#nextwinclientprotocol)
- [Nextwin.Client.UI](#nextwinclientui)
- [Nextwin.Client.Util](#nextwinclientutil)

## MessagePack
서버와 통신할 때 데이터를 직렬화, 역직렬화 하기 위해 사용되는 라이브러리입니다.
### 데이터 패킷 클래스 정의
우선 프레임워크에 종속적인 데이터 패킷 클래스(서버와 주고 받기 위해 직렬화할 클래스)를 만들기 위해 다음과 같이 [Nextwin.Client.Protocol.SerailizableData](#serializabledata)를 상속받는 클래스를 생성합니다.
```C#
using MessagePack;
using Nextwin.Client.Protocol;
using System.Collections.Generic;

// 전송 데이터 패킷임을 표시하기 위해 [MessagePackObject]를 붙여줍니다.
[MessagePackObject]
public class PacketExample : SerializableData
{
    // 직렬화할 필드 변수 위에 [Key(n)]을 작성합니다. Key는 1번부터 시작하세요.
    [Key(1)]
    public int IntData { get; set; }
    [Key(2)]
    public float FloatData { get; set; }
    [Key(3)]
    public List<int> IntList { get; set; }
    [Key(4)]
    public string StringData { get; set; }

    // SerializableData를 상속받기 때문에 다음과 같은 형태의 생성자가 반드시 필요합니다.
    public Packet(int msgType) : base(msgType)
    {
    }
}
```

### 직렬화 및 역직렬화를 위한 코드 자동 생성
다음 작업은 데이터 패킷 클래스가 추가된 후 빌드나 유니티 에디터에서 실행시키기 전에 반드시 수행되어야 합니다. 우선 [.NET Core 3.1버전을 설치](https://dotnet.microsoft.com/download)하세요. cmd나 터미널에서 ```dotnet -v``` 명령어를 통해 설치된 .NET 버전을 확인할 수 있습니다. 버전이 .NET Core 3.1인지 꼭 확인하세요. 유니티 실행 시에는 동적으로 코드를 생성할 수 없기 때문에 데이터 패킷 클래스가 추가될때마다 다음 절차를 반드시 수행해주세요.

유니티 에디터 메뉴의 Window->MessagePack->CodeGenerator를 선택하여 MessagePackCodeGen창을 엽니다.

<img src="https://user-images.githubusercontent.com/44297538/103760921-28197300-5059-11eb-897d-053b1df31b77.png" width="55%" height="55%" title="Code Gen" alt="Code Gen"></img>

첫 입력 줄에는 프로젝트 어셈블리 파일을 입력합니다. 기본 경로는 ${유니티 프로젝트}/Assets 폴더입니다. 두번째 줄에는 생성된 코드가 위치할 경로를 입력합니다. 아래의 Generate 버튼을 클릭하면 다음과 같이 두번째 줄에 지정한 위치에 스크립트가 생성된 것을 확인할 수 있습니다.

<img src="https://user-images.githubusercontent.com/44297538/103760927-2b146380-5059-11eb-8821-f6319f6769b0.png" width="60%" height="60%" title="Generated" alt="Generated"></img>

기타 자세한 내용은 [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp.git)을 참고하세요.

## Nextwin.Client.Game
### PlayerController
캐릭터 객체에 PlayerController Component를 추가하여 캐릭터를 조작할 수 있습니다. 인스펙터창을 보면 다음과 같습니다.

<img src="https://user-images.githubusercontent.com/44297538/103763370-1043ee00-505d-11eb-831f-129f09ed1da6.png" width="60%" height="60%" title="PC" alt="PC"></img>

On Control을 체크하면 사용자의 입력이 캐릭터의 움직임에 영향을 줍니다. 캐릭터를 조작하기 위해서는 On Control이 체크되어 있어야 합니다.

"Body"는 캐릭터의 머리, 팔, 다리, 몸통을 모두 포함한 객체입니다. 사용자의 입력을 받아 실제로 움직이는 오브젝트입니다. "Pivot"은 카메라 조작을 위해 사용됩니다. 빈 게임 오브젝트이고 캐릭터를 조작하는 방법(시점)에 따라 달라집니다. 아래에서 어떤 오브젝트가 "Pivot"에 할당되어야 하는지 설명합니다. "Camera"에 알맞은 카메라를 넣어줍니다. 게임을 실행하면 카메라에 [CameraController](#cameracontroller) 컴포넌트가 추가되고 카메라에 관한 설정은 이를 통해 할 수 있습니다. "Foot"은 캐릭터의 발입니다. 발이 땅에 닿아있는지, 즉 캐릭터가 공중에 떠있는지 땅 위에 있는지 확인하기 위해 사용됩니다. "Foot"에는 Rigidbody 컴포넌트가 붙어있어야 합니다.

"Use Mouse"는 캐릭터 조작에 마우스를 사용하는지 여부입니다. "Use Mouse" 상태에 따라 게임의 시점과 조작 방법, 그리고 캐릭터의 계층 구조가 달라집니다. "Mouse Sensitivity"는 "Use Mouse"를 체크한 상태일 때만 유효하며 마우스 민감도를 설정하는 항목입니다.

[Control Basic Key Setting] 에서는 기본 조작키를 설정할 수 있습니다.

"Walk Speed"와 "Run Speed"는 각각 기본 움직임 시 속도, "Run Key"를 이동키와 함께 눌렀을 때의 속도입니다. "Rotate Speed"는 "Use Mouse"가 체크되어있지 않을 때 유효하며 캐릭터가 이동하는 방향 전환 시 캐릭터가 회전하는 속도입니다. "Jump Power"를 통해 점프력을 설정할 수 있습니다.

"Check Ground By Tag Or Layer"은 땅 오브젝트를 판별할 때 Tag로 검사할 것인지, Layer로 검사할 것인지 여부입니다. "Ground Tag Or Layer"에는 땅 오브젝트의 Tag("Check Ground By Tag Or Layer"에서 Tag를 선택했을 때) 혹은 Layer("Check Ground By Tag Or Layer"에서 Layer를 선택했을 때)를 입력합니다.

PlayerController를 상속받아 몇몇 함수를 재정의하거나 다른 기능을 추가하여 필요에 맞게 튜닝해서 사용할 수 있습니다.

캐릭터는 2가지 방법으로 조작할 수 있습니다.

#### 1. 마우스를 사용하여 조작하는 방법 - 숄더뷰(Shoulder View) 혹은 백뷰(Back View)
![mouse](https://user-images.githubusercontent.com/44297538/103766808-d37af580-5062-11eb-810a-10cc9216c4b2.gif)

마우스를 사용할 때에는 다음과 같이 캐릭터를 구성해줍니다.

<img src="https://user-images.githubusercontent.com/44297538/103767029-31a7d880-5063-11eb-8337-928dfe13e000.png" width="40%" height="40%" title="hi" alt="hi"></img>

"Body"는 캐릭터의 머리, 팔, 다리, 몸통을 모두 포함한 객체입니다. "Pivot"은 빈 게임 오브젝트입니다.

#### 2. 마우스 없이 조작하는 방법 - 쿼터뷰(Quarter View) 혹은 사이드뷰(Side View)
![nomouse](https://user-images.githubusercontent.com/44297538/103766817-d544b900-5062-11eb-8153-ffc24e41c5c1.gif)

마우스를 사용하지 않을 때에는 다음과 같이 캐릭터를 구성해줍니다.

<img src="https://user-images.githubusercontent.com/44297538/103767457-01ad0500-5064-11eb-9e14-9a59c05e746d.png" width="30%" height="30%" title="hi2" alt="hi2"></img>

### CameraController
플레이어와 카메라의 거리, 카메라의 높이 및 각도를 설정할 수 있습니다.

![cam](https://user-images.githubusercontent.com/44297538/103767955-ebec0f80-5064-11eb-9216-6a977bc62b3e.gif)

"Camera Distance"는 플레이어와 카메라 사이의 거리입니다. "Camera Height"는 카메라의 높이입니다. "Camera Angle"은 카메라의 각도입니다. "Camera Speed"는 PlayerController의 Walk Speed와 비슷하거나 약간 더 느리게 하는 것이 좋습니다.

### GameManagerBase
서버와 통신이 필요한 경우 GameManager를 만들 때 Nextwin.Client.Game.GameManagerBase를 상속받으세요. 서버와 연결 및 서버로부터 데이터를 수신하는 등의 번거로운 작업을 코딩하지 않아도 됩니다. 다음은 서버로부터 받은 데이터를 처리하는 예시입니다.
```C#
using Nextwin.Client.Game;
using Nextwin.Client.Protocol;

public class GameManager : GameManagerBase<GameManager>
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // 추가적인 Start 함수 구현
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // 추가적인 Update 함수 구현
    }

    protected override void OnReceivedData(int msgType, byte[] receivedData)
    {
        switch(msgType)
        {
            case ExProtocol.ExMessage:
                PacketExample packetExample = Serializer.Instance.Deserialize<PacketExample>(receivedData);
                /*
                 * 적절한 작업을 수행합니다.
                 */
                break;

             .....
        }
    }
}
```
Start와 Update 함수의 접근자를 protected로 바꿔주고 override 키워드를 추가합니다. 각 함수의 첫 줄에는 ```base.Start()```와 ```base.Update()```함수를 추가하여 GameManagerBase의 Start, Update 함수가 수행되도록 합니다. OnReceivedData 함수를 반드시 구현해야 합니다. 서버로부터 데이터를 받았을 때 호출되며 위의 예시와 같이 msgType에 따라 적절한 작업이 수행되도록 합니다. receivedData를 ```Serializer.Instance.Deserilalize<특정 패킷 클래스>(receivedData)```와 같이 역직렬화할 수 있습니다. 물론 이 작업을 위해서 해당 패킷 클래스를 작성한 후 [동적 코드 생성 작업](#messagepack)을 해주어야 합니다.

GameManagerBase를 상속받은 GameManager는 [Singleton](#singleton) 객체입니다.

## Nextwin.Client.Protocol
### SerializableData
서버에 어떤 데이터를 주고받을 때에는 그 데이터를 직렬화, 역직렬화 해야합니다. 이때 직렬화, 역직렬화 되는 데이터 클래스의 최상위 클래스입니다. SerializableData를 상속받지 않고도 직렬화 가능한 클래스를 만들 수 있지만 해당 프레임워크에 맞추어 사용하기 위해 SerializableData를 상속받습니다. 직렬화 및 역직렬화 가능한 데이터 클래스를 만드는 방법은 [MessagePack](#messagepack)에서 확인하세요.

### Serializer
[Singleton](#singleton) 객체로 Nextwin.Protocol.ISerializer 인터페이스를 구현합니다. 데이터를 직렬화하고 역직렬화 하는 방법은 다음과 같습니다.
```C#
byte[] bytes = Serializer.Instance.Serialize(packetExample1);
PacketExample packetExample2 = Serializer.Instance.Deserialize<PacketExample>(bytes);
```
