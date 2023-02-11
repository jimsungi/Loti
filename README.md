# TigerL10N
L10N resource creation tool (localization resource file creator)

This is not productive stage - early idea stage - it's no help yet

Idears
- create resource file per CLR
- from source file, extract all string and make it resource
- may be support xaml too
- xaml component content to be attributed (likely from <Button>Run</Button>  to <Button Content="Run"></Button>, but <Button Content="{res:TigerL10N.btn_run}"></Button>
- src to res, res to src conversion might be supported

- may be global resource might be supported too

- language files created
- resource to excel for tanslation tool (or tool supported format, maybe trados, transit, etc)

- exceptional process support - by text itself with condition (file, comment - like // TLException Korea ), by idenfication rule (predefined prefix like txt, chbox, etc), by Xaml attribute like { preceded text,
- use humanizer naming suggestions / maybe not(humanizer use English, but I prefer Korean) or optional?

- providing UI, user can decide how to process
- providing UI, like translation tool (load excel, edit data, Qa result) - It can be a grid like excel

- simple library to load resource file - can be call L10N Bridge
- L10N Bridge or middlewear : languages from static L10N resource, runtime L10N resource, database L10N resource
- L10N Bridge or middlewear : buffering / priority

돈버는중인 프로젝트에서 갑자기 다국어버전이 필요하다고 해서... 프레임워크 설계한 양반은 다국어버전 고려하지 않았는데다 관리자는 번역을 외부업체에 맡기는 방법이나 절차를 고려하지 않고 있는 상황이다. 일단 내 시간을 절약하기 위해 프레임워크들 찾아봤는데, 행복하게 시간을 절약할 방법은 없는 것 같다. 일단 프로젝트 특성에 맡게 짧은 코드를 짰는데... 관리자가 갑자기 포기했다. 

아이디어 몇가지 나온 김에 쓸만한 툴 (TigerOCR 번역하려니 귀찮아서)을 만들어 볼 생각이다. 소스에서 리소스 만들어 놓고, 리소스 쓸 수 있게 소스를 수정한 다음, 번역을 쓱 맡긴다. 그리고는 번역 안된 버전을 다시 만들 수 있으면 좋겠다. (그럴려면 리소스 만들고 컴파일 버전 소스 만든 다음에, 여기에서 원래 소스가 100% 똑같이 나와야 하니까 신경쓸게 많지만, 코딩 작업은 행복해 질 것이다.)

내 첫 프로그래머 커리어는 번역회사에서 시작했다. In house tool이라고 불렀는데, 그냥 번역 빨리할 수 있게 도와주는 자잘한 툴을 많이 만들었었다. 이것이 그런 In house tool이 될 것이다. Trados나 Transit이나 그런 In house tool이 발전해서 생긴 프로그램이다. 

Library
- For Folder Open Dialog  https://github.com/ookii-dialogs/ookii-dialogs-wpf (https://www.ookii.org/software/dialogs/)
- ConfigNet for easy user setting https://github.com/aloneguid/config

