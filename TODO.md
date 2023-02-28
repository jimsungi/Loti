# TigerL10N TODO
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

