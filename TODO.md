# TigerL10N TODO

Big Idea
- create resource file per CLR (o)
- from source file, extract all string and make it resource (o)
- may be support xaml too (-)
- xaml component content to be attributed (likely from &<;Button&>;Run&<;/Button>  to <Button Content="Run"></Button>, but <Button Content="{res:TigerL10N.btn_run}"></Button> (-)
- src to res, res to src conversion might be supported (-)

- may be global resource might be supported too (-)

- language files created (o)
- resource to excel for tanslation tool (or tool supported format, maybe trados, transit, etc)

- exceptional process support - by text itself with condition (file, comment - like // TLException Korea ), by idenfication rule (predefined prefix like txt, chbox, etc), by Xaml attribute like { preceded text,(-)
- use humanizer naming suggestions / maybe not(humanizer use English, but I prefer Korean) or optional? (?)

- providing UI, user can decide how to process (-)
- providing UI, like translation tool (load excel, edit data, Qa result) - It can be a grid like excel (-)

- simple library to load resource file - can be call L10N Bridge (-)
- L10N Bridge or middlewear : languages from static L10N resource, runtime L10N resource, database L10N resource (-)
- L10N Bridge or middlewear : buffering / priority (-)

- .L10N extension and same as solution filename : tool can select visual studio solution then go (make it vs thing) 
- visual studio integration (create l10n project,etc)

- TM support (may be I can refer ResXManager)

Small things

* Version things ... Is there any library or code or tools. If code or anything, it can be integrated.(-)
* Shortcut works into ListView of resource editor (o)
* Need way of 1) add / 2) remove / 3) copy / 4) select Language easier. (o)
* Your Id library can be loaded into id library and reused. (o)
* Your Id library can be saved as Global library (and there might be a way to use default Id Library) (o)
* Need some ".tmp" files. If any *.tmp file exists need some warning (o)
* L10N folder OK. resource name would be l.resx (o)
* Solution based translation (o)
* Translation option of (L.resource, G.id, M.message)
* M.message as network (sample one file .ASP and .PHP page, file based and MariaDB based) 
* Test cases need
* Need to specipy project NameSpace 
* Select Tree Node to display words specified to that file
* Load G.cs when create / open project
* Term specification is need (Solution, Project, Translation, etc)
* Translation Window might be splitted, especially Word editor
* Adding [As Message] column to Resources Window
