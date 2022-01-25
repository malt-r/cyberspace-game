#!/bin/sh

pandoc \
	-t latex Projektbericht.md --number-sections -V papersize=a4paper \
	-V lang=de-DE --pdf-engine=lualatex -V fontfamily=libertine \
	-V monofont=inconsolata -V fontsize=12pt -V breakurl -V hyphens=URL \
	-V colorlinks \
	--template eisvogel \
	--citeproc \
	--toc \
	-o Projektbericht.pdf
