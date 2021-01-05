# Unity Client Framework
[![Download](https://img.shields.io/badge/download-v2.1.2-blue)](https://github.com/nextwingames/unity-client/releases/download/2.1.2/NextwinUnityClient2.1.2.unitypackage)
[![license](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/nextwingames/unity-client/blob/main/LICENSE)

유니티 게임 클라이언트 개발 프레임워크입니다. 다양한 기능을 적은 코딩으로 쉽게 구현할 수 있습니다. C# 서버와 통신 시 직렬화를 위해 [MessagePack-C#](https://github.com/neuecc/MessagePack-CSharp.git)을 사용하였으며 기타 유틸을 사용하기 위해 [Nextwin C# Library](https://github.com/nextwingames/csharp-lib.git)를 참조합니다. 또한 C#의 Dictionary를 유니티 인스펙터창에서 수정할 수 있도록 [Serializable Dictionary Asset](https://assetstore.unity.com/packages/tools/integration/serializabledictionary-90477)을 포함합니다. 

이 프레임워크를 사용하기 위해 상단의 download 버튼을 클릭하여 유니티 패키지를 다운받은 후, 유니티 프로젝트에 import 하세요.

## Contents
- [MessagePack](#messagepack)
- [Nextwin.Client.Game](#nextwinclientgame)
- [Nextwin.Client.Protocol](#nextwinclientprotocol)
- [Nextwin.Client.UI](#nextwinclientui)
- [Nextwin.Client.Util](#nextwinclientutil)

## MessagePack
서버와 통신할 때 데이터를 직렬화, 역직렬화 하기 위해 사용되는 라이브러리입니다. 우선 프레임워크에 종속적인 데이터 패킷 클래스(서버와 주고 받기 위해 직렬화할 클래스)를 만들기 위해 다음과 같이 [Nextwin.Client.Protocol.SerailizableData]를 상속받는 클래스를 생성합니다.
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

다음 작업은 데이터 패킷 클래스가 추가된 후 빌드나 유니티 에디터에서 실행시키기 전에 반드시 수행되어야 합니다. 우선 [.NET Core 3.1버전을 설치](https://dotnet.microsoft.com/download)하세요. cmd나 터미널에서 ```dotnet -v``` 명령어를 통해 설치된 .NET 버전을 확인할 수 있습니다. 유니티 실행 시에는 동적으로 코드를 생성할 수 없기 때문에 데이터 패킷 클래스가 추가될때마다 다음 절차를 반드시 수행해주세요.

기타 자세한 내용은 [MessagePack-CSharp.git](https://github.com/neuecc/MessagePack-CSharp.git)을 참고하세요.
