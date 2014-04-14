TOP := $(shell pwd)
RESGEN=resgen
BOOTSTRAP_MCS=/Users/shahi/mono/bin/bin/mcs -sdk:4.5
BOOTSTRAP_DIR=$(TOP)/bootstrap
CSC=~/mono/bin/bin/mono $(BOOTSTRAP_DIR)/rcsc.exe
OUTPUT_DIR=$(TOP)/rcsc
FACADES_DIR=/Users/shahi/mono/bin/lib/mono/4.5/Facades

resources:
	cd Src/Compilers/Core/Source/ && $(RESGEN) CodeAnalysisResources.resx Microsoft.CodeAnalysis.CodeAnalysisResources.resources
	cd Src/Compilers/CSharp/Source/ && $(RESGEN) CSharpResources.resx Microsoft.CodeAnalysis.CSharp.CSharpResources.resources

bootstrap: resources
	mkdir -p $(BOOTSTRAP_DIR)
	cd Src/Tools/Source/CompilerGeneratorTools/Source/BoundTreeGenerator && $(BOOTSTRAP_MCS) -out:$(BOOTSTRAP_DIR)/BoundTreeGenerator.exe BoundNodeClassWriter.cs \
	Model.cs Program.cs
	cd Src/Compilers/CSharp/Source/BoundTree && mono $(BOOTSTRAP_DIR)/BoundTreeGenerator.exe CSharp BoundNodes.xml BoundNodes.xml.Generated.cs
	cd Src/Tools/Source/CompilerGeneratorTools/Source/CSharpErrorFactsGenerator && $(BOOTSTRAP_MCS) -out:$(BOOTSTRAP_DIR)/CSharpErrorFactsGenerator.exe Program.cs
	cd Src/Compilers/CSharp/Source/Errors && mono $(BOOTSTRAP_DIR)/CSharpErrorFactsGenerator.exe ErrorCode.cs ErrorFacts.Generated.cs
	cd Src/Tools/Source/CompilerGeneratorTools/Source/CSharpSyntaxGenerator && $(BOOTSTRAP_MCS) -out:$(BOOTSTRAP_DIR)/CSharpSyntaxGenerator.exe *.cs
	cd Src/Compilers/CSharp/Source/Syntax && mono $(BOOTSTRAP_DIR)/CSharpSyntaxGenerator.exe Syntax.xml Syntax.xml.Generated.cs
	cd Src/Compilers/Core/Source/ && $(BOOTSTRAP_MCS) -t:library -out:$(BOOTSTRAP_DIR)/Microsoft.CodeAnalysis.dll -unsafe -d:BOOTSTRAP -d:COMPILERCORE -noconfig \
		-r:$(FACADES_DIR)/System.Runtime.dll -r:../../../References/Immutable/System.Collections.Immutable.dll \
		-r:../../../References/MetadataReader/System.Reflection.Metadata.dll -r:$(FACADES_DIR)/System.Collections.dll  @files.lst \
		-r:System.Core -r:System -r:System.Xml -r:System.Xml.Linq -r:$(FACADES_DIR)/System.Reflection.Primitives.dll \
		-r:$(FACADES_DIR)/System.IO.dll -resource:Microsoft.CodeAnalysis.CodeAnalysisResources.resources
	cd Src/Compilers/CSharp/Source/ && $(BOOTSTRAP_MCS) -t:library -out:$(BOOTSTRAP_DIR)/Microsoft.CodeAnalysis.CSharp.dll -unsafe -d:BOOTSTRAP -noconfig \
		-r:$(FACADES_DIR)/System.Runtime.dll -r:../../../References/Immutable/System.Collections.Immutable.dll \
		-r:../../../References/MetadataReader/System.Reflection.Metadata.dll -r:$(BOOTSTRAP_DIR)/Microsoft.CodeAnalysis.dll @files.lst \
		-r:System.Core -r:System -r:System.Xml -r:System.Xml.Linq -resource:Microsoft.CodeAnalysis.CSharp.CSharpResources.resources
	cd Src/Compilers/CSharp/rcsc/ && $(BOOTSTRAP_MCS) -out:$(BOOTSTRAP_DIR)/rcsc.exe Csc.cs Program.cs \
		-r:$(BOOTSTRAP_DIR)/Microsoft.CodeAnalysis.dll -r:$(BOOTSTRAP_DIR)/Microsoft.CodeAnalysis.CSharp.dll \
		-r:../../../References/Immutable/System.Collections.Immutable.dll -r:$(FACADES_DIR)/System.Runtime.dll
	cp Src/References/Immutable/System.Collections.Immutable.dll $(BOOTSTRAP_DIR)
	cp Src/References/MetadataReader/System.Reflection.Metadata.dll $(BOOTSTRAP_DIR)

rcsc: $(OUTPUT_DIR)/rcsc.exe

$(OUTPUT_DIR)/rcsc.exe: $(BOOTSTRAP_DIR)/rcsc.exe
	mkdir -p $(OUTPUT_DIR)
	cd Src/Compilers/Core/Source/ && $(CSC) -t:library -out:$(OUTPUT_DIR)/Microsoft.CodeAnalysis.dll -unsafe -d:COMPILERCORE -noconfig \
		-r:$(FACADES_DIR)/System.Runtime.dll -r:../../../References/Immutable/System.Collections.Immutable.dll \
		-r:../../../References/MetadataReader/System.Reflection.Metadata.dll -r:$(FACADES_DIR)/System.Collections.dll  @files.lst \
		-r:System.Core.dll -r:System.dll -r:System.Xml.dll -r:System.Xml.Linq.dll -r:$(FACADES_DIR)/System.Reflection.Primitives.dll \
		-r:$(FACADES_DIR)/System.IO.dll -resource:Microsoft.CodeAnalysis.CodeAnalysisResources.resources
	cd Src/Compilers/CSharp/Source/ && $(CSC) -t:library -out:$(OUTPUT_DIR)/Microsoft.CodeAnalysis.CSharp.dll -unsafe -noconfig \
		-r:$(FACADES_DIR)/System.Runtime.dll -r:../../../References/Immutable/System.Collections.Immutable.dll \
		-r:../../../References/MetadataReader/System.Reflection.Metadata.dll -r:$(OUTPUT_DIR)/Microsoft.CodeAnalysis.dll @files.lst \
		-r:System.Core.dll -r:System.dll -r:System.Xml.dll -r:System.Xml.Linq.dll -resource:Microsoft.CodeAnalysis.CSharp.CSharpResources.resources
	cd Src/Compilers/CSharp/rcsc/ && $(CSC) -out:$(OUTPUT_DIR)/rcsc.exe Csc.cs Program.cs \
		-r:$(OUTPUT_DIR)/Microsoft.CodeAnalysis.dll -r:$(OUTPUT_DIR)/Microsoft.CodeAnalysis.CSharp.dll \
		-r:../../../References/Immutable/System.Collections.Immutable.dll -r:$(FACADES_DIR)/System.Runtime.dll \
		-r:System.Core.dll
	cp Src/References/Immutable/System.Collections.Immutable.dll $(OUTPUT_DIR)
	cp Src/References/MetadataReader/System.Reflection.Metadata.dll $(OUTPUT_DIR)
	cp Src/Compilers/CSharp/rcsc/*.rsp $(OUTPUT_DIR)

use-roslyn:
	sed "s,/Users/shahi/mono/bin/lib/mono/4.5/mcs.exe,`pwd`/rcsc/rcsc.exe," < /usr/bin/mcs > tmp
	sed "s,/Users/shahi/mono/bin/lib/mono/4.5/mcs.exe,`pwd`/rcsc/rcsc.exe," < /Users/shahi/mono/bin/bin/mcs > tmp2
	chmod +x tmp tmp2
	sudo sh -c "(cp tmp2 /Users/shahi/mono/bin/bin/mcs; sudo cp tmp /usr/bin/mcs)"


undo:
	sudo sh -c "(cp /Users/shahi/mono/bin/bin/backup-mcs /Users/shahi/mono/bin/bin/mcs; cp /usr/bin/mcs-backup /usr/bin/mcs)"
