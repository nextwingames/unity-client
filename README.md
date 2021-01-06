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
우선 프레임워크에 종속적인 데이터 패킷 클래스(서버와 주고 받기 위해 직렬화할 클래스)를 만들기 위해 다음과 같이 [Nextwin.Client.Protocol.SerailizableData]를 상속받는 클래스를 생성합니다.
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
캐릭터 객체에 PlayerController Component를 추가하여 캐릭터를 조작할 수 있습니다. 캐릭터는 2가지 방법으로 조작할 수 있습니다.

#### 1. 마우스를 사용하여 조작하는 방법 - 숄더뷰(Shoulder View) 혹은 백뷰(Back View)


#### 2. 마우스 없이 조작하는 방법 - 쿼터뷰(Quarter View)
