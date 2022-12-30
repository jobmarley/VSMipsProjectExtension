import re
import os
import sys
import xml.etree.ElementTree as ET

idl_filepath = r'ElfSymbolProvider.idl'
manifest_filepath = r'ElfSymbolProvider.manifest'

def tokenize(text):
	return [(x.start(0), x[0]) for x in re.finditer('((?:\s*)|(?://[^\n]*)|(?:"[^"]*")|(?:\w+)|.)', text)]

def index_to_line_col(s, index):
    """Returns (line_number, col) of `index` in `s`."""
    if not len(s):
        return 1, 1
    sp = s[:index+1].splitlines(keepends=True)
    return len(sp), len(sp[-1])

def get_idl_interface_list(filepath):
	with open(filepath, 'r') as f:
		text = f.read()
		tokens = tokenize(text)
		tokens = [t for t in tokens if not t[1].isspace() and not t[1].startswith('//') and not t[1] == '']

		l = []
		i = 0
		while i < len(tokens):
			start, t = tokens[i]
			if t == 'interface' and i < len(tokens) + 1:
				l.append((index_to_line_col(text, tokens[i+1][0]), tokens[i+1][1]))
			elif t == 'coclass':
				i = i + 2
				if i < len(tokens) and tokens[i][1] == '{':
					i = i + 1
					depth = 1
					while i < len(tokens):
						start, t = tokens[i]
						if t == '}':
							depth = depth - 1
							if depth == 0:
								break
						elif t == '{':
							depth = depth + 1
						i = i + 1
			i = i + 1
		return l

def get_manifest_interface_list(filepath):
	tree = ET.parse(filepath)
	root = tree.getroot()
	result = []
	for e in root.findall('.//{urn:schemas-microsoft-com:asm.v1}comInterfaceExternalProxyStub'):
		try:
			result.append(e.attrib['name'])
		except Exception as e:
			pass
	return result

l1 = get_idl_interface_list(idl_filepath)
l2 = get_manifest_interface_list(manifest_filepath)

error = False
for x in l1:
	if not x[1] in l2:
		print('{}({},{}) : Command line error MC0001 : comInterfaceExternalProxyStub not found for interface {} in {}'.format(idl_filepath, *x[0], x[1], manifest_filepath))
		error = True

if error:
	sys.exit(1)
sys.exit(0)