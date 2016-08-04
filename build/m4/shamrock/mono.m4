AC_DEFUN([SHAMROCK_FIND_MONO_COMPILER],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, mcs)
])

AC_DEFUN([SHAMROCK_FIND_MONO_RUNTIME],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MONO, mono)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE],
[
	PKG_CHECK_MODULES(MONO_MODULE, mono >= $1)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE_NOBAIL],
[
	PKG_CHECK_MODULES(MONO_MODULE, mono >= $1, 
		HAVE_MONO_MODULE=yes, HAVE_MONO_MODULE=no)
	AC_SUBST(HAVE_MONO_MODULE)
])

AC_DEFUN([_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES],
[
	for asm in $(echo "$*" | cut -d, -f2- | sed 's/\,/ /g')
	do
		AC_MSG_CHECKING([for Mono $2 GAC for $asm.dll])
		libdir="$($PKG_CONFIG --variable=libdir $1)"
		prefix="$($PKG_CONFIG --variable=prefix $1)"
		if test \
			-e "$libdir/mono/$2/$asm.dll" -o \
			-e "$prefix/lib/mono/$2/$asm.dll"; \
			then \
			AC_MSG_RESULT([found])
		elif test \
			-e "$libdir/mono/$2-api/$asm.dll" -o \
			-e "$prefix/lib/mono/$2-api/$asm.dll"; \
			then \
			AC_MSG_RESULT([found])
		else
			AC_MSG_RESULT([not found])
			AC_MSG_ERROR([missing required Mono $2 assembly: $asm.dll])
		fi
	done
])

AC_DEFUN([SHAMROCK_CHECK_MONO_1_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(1.0, $*)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(2.0, $*)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_4_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(4.0, $*)
])


