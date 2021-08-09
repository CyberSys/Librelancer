# Run with python3 gen.py
# MIT License - Copyright (c) Callum McGing
# This file is subject to the terms and conditions defined in
# LICENSE, which is part of this source code package

import json
from datetime import datetime,timezone

print("Librelancer Protocol Generator 2021-07-16")

# NetPacketReader methods
typeMethods = {
  "int" : "GetInt",
  "Quaternion" : "GetQuaternion",
  "Vector3" : "GetVector3",
  "byte" : "GetByte",
  "sbyte" : "GetSByte",
  "double" : "GetDouble",
  "float" : "GetFloat",
  "long" : "GetLong",
  "IPEndPoint" : "GetNetEndPoint",
  "short" : "GetShort",
  "string" : "GetStringPacked",
  "uint" : "GetUInt",
  "ulong" : "GetULong",
  "ushort" : "GetUShort",
  "bool" : "GetBool",
}

# Read the json
with open("protocol.json", "r") as f:
  jstr = f.read()
  
jfile = json.loads(jstr)

# Generator Helper
varidx = 0
def get_varname():
  global varidx
  varidx += 1
  return "_" + str(varidx)
  
# File output code
outfile = open("Protocol.gen.cs", "w")
tabs = 0

def get_indent():
  _indent = ""
  for i in range(tabs):
    _indent = _indent + "    "
  return _indent
    
def writeline(line):
  outfile.write(get_indent())
  outfile.write(line)
  outfile.write("\n")
  
def writestart(line):
  outfile.write(get_indent())
  outfile.write(line)
  
def write(line):
  outfile.write(line)
  
def writeend(line):
  outfile.write(line)
  outfile.write("\n")
 
def whitespace():
  outfile.write("\n")

return_packets = []

# Creates a C# method prototype from JSON description
def method_proto(method, _class):
  _proto = ""
  if _class:
    _proto = "public "
  if "return" not in method:
    _proto += "void "
  else:
    _proto += "Task<" + method["return"] + "> "
  _proto = _proto + method["name"] + "("
  if "args" not in method:
    return _proto + ")"
  for idx, a in enumerate(method["args"]):
    _proto += a["type"] + " " + a["name"]
    if idx + 1 < len(method["args"]):
      _proto += ", "
  _proto += ")"
  return _proto

def remote_method(mthd, prefix):
  global tabs
  whitespace()
  writeline(method_proto(mthd, True))
  writeline("{")
  tabs += 1
  if "return" in mthd:
    writeline("var complete = ResponseHandler.GetCompletionSource_" + mthd["return"] + "(retSeq);")
    if mthd["return"] not in return_packets:
        return_packets.append(mthd["return"])
  writeline("SendPacket(new " + prefix + "Packet_" + mthd["name"] + "() {")
  tabs += 1
  if "return" in mthd:
    writeline("Sequence = Interlocked.Increment(ref retSeq),")
  if "args" in mthd:
    for x in mthd["args"]:
      writeline(x["name"] + " = " + x["name"] + ",")
  tabs -= 1
  writeline("});")
  if "return" in mthd:
    writeline("return complete.Task;")
  tabs -= 1
  writeline("}")
  


# Header
writeline("// AUTOGENERATED CODE")
writeline("// Generated: " + datetime.now(timezone.utc).strftime("%Y%m%d %H:%M:%S UTC"))
writeline("// Method count: " + str(len(jfile["client_methods"]) + len(jfile["server_methods"])))
whitespace()
writeline("using System;")
writeline("using System.Numerics;")
writeline("using System.Threading;")
writeline("using System.Threading.Tasks;")
writeline("using LiteNetLib;")
writeline("using LiteNetLib.Utils;")
whitespace()
writeline("namespace LibreLancer.Net")
writeline("{")
tabs += 1
#
# Client Interface
#
writeline("public interface IClientPlayer")
writeline("{")
tabs += 1
for mthd in jfile["client_methods"]:
  writeline(method_proto(mthd, False) + ";")
tabs -= 1
writeline("}")
whitespace()

#
# Server Interface
#
writeline("public interface IServerPlayer")
writeline("{")
tabs += 1
for mthd in jfile["server_methods"]:
  writeline(method_proto(mthd, False) + ";")
tabs -= 1
writeline("}")
whitespace()


#
# Client IServerPlayer implementation
#
writeline("public partial class RemoteServerPlayer : IServerPlayer")
writeline("{")
tabs += 1
# Fields
writeline("int retSeq;");
# Generate
for mthd in jfile["server_methods"]:
  remote_method(mthd, "Server")
tabs -= 1
whitespace()
writeline("}")

#
# Server IClientPlayer implementation
#
writeline("public partial class RemoteClientPlayer : IClientPlayer")
writeline("{")
tabs += 1
# Fields
writeline("int retSeq;");
# Generate
for mthd in jfile["client_methods"]:
  remote_method(mthd, "Client")
tabs -= 1
whitespace()
writeline("}")



#
# Packet Definitions
#
def Packet(mthd, classname):
  global tabs
  whitespace()
  writeline("public class " + classname + " : IPacket");
  writeline("{")
  tabs += 1
  # Fields
  if "return" in mthd:
    writeline("public int Sequence;")
  if "args" in mthd:
    for a in mthd["args"]:
      writeline("public " + a["type"] + " " + a["name"] + ";")
  # Read Packet
  def read_expr(name, type):
    if type in typeMethods:
        writeline(name + " = message." + typeMethods[type] + "();")
    else:
        writeline(name + " = " + type + ".Read(message);")
  writeline("public static object Read(NetPacketReader message)")
  writeline("{")
  tabs += 1
  writeline("var _packet = new " + classname + "();")
  if "return" in mthd:
      writeline("_packet.Sequence = message.GetInt();")
  if "args" in mthd:
      for a in mthd["args"]:
        if "[]" in a["type"]:
            single_type = a["type"].replace("[","")
            single_type = single_type.replace("]","")
            # Null arrays are written with length 0
            # Otherwise length + 1
            writeline("uint __len_" + a["name"] + " = message.GetVariableUInt32();")
            writeline("if (__len_" + a["name"] + " > 0) {")
            tabs += 1
            writeline("_packet." + a["name"] + " = new " + single_type + "[(int)(__len_" + a["name"] + " - 1)];")
            writeline("for(int _ARRIDX = 0; _ARRIDX < _packet." + a["name"] + ".Length; _ARRIDX++)")
            tabs += 1
            read_expr("_packet." + a["name"] + "[_ARRIDX]", single_type)
            tabs -= 1
            tabs -= 1
            writeline("}")
        else:
            read_expr("_packet." + a["name"], a["type"])
  writeline("return _packet;")
  tabs -= 1
  writeline("}")
  
  def put_single(name, type):
    if type == "string":
        writeline("message.PutStringPacked(" + name + ");")
    elif type in typeMethods:
        writeline("message.Put(" + name + ");")
    else:
        writeline(name + ".Put(message);")
    
  # Write Packet
  writeline("public void WriteContents(NetDataWriter message)")
  writeline("{")
  tabs += 1
  if "return" in mthd:
    writeline("message.Put(Sequence);")
  if "args" in mthd:
    for a in mthd["args"]:
      if "[]" in a["type"]:
        single_type = a["type"].replace("[","")
        single_type = single_type.replace("]","")
        # Null arrays are written with length 0
        # Otherwise length + 1
        writeline("if (" + a["name"] + " != null) {")
        tabs += 1
        writeline("message.PutVariableUInt32((uint)(" + a["name"] + ".Length + 1));")
        writeline("foreach(var _element in " + a["name"] + ")")
        tabs += 1
        put_single("_element", single_type)
        tabs -= 1
        tabs -= 1
        writeline("} else {")
        writeline("    message.PutVariableUInt32(0);")
        writeline("}")
      else:
        put_single(a["name"], a["type"])
  tabs -= 1
  writeline("}")
  tabs -= 1
  writeline("}")


for mthd in jfile["server_methods"]:
  classname = "ServerPacket_" + mthd["name"]
  Packet(mthd, classname)

for mthd in jfile["client_methods"]:
  classname = "ClientPacket_" + mthd["name"]
  Packet(mthd, classname)

#
# Response packets
#
for t in return_packets:
    Packet({ "return": "yes", "args": [ { "name": "Value", "type": t } ] }, "ResponsePacket_" + t)


  
#
# Completion Sources
#
writeline("public partial class NetResponseHandler")
writeline("{")
tabs += 1
# GetCompletionSource_ 
for t in return_packets:
  writeline("public TaskCompletionSource<" + t + "> GetCompletionSource_" + t + "(int sequence)")
  writeline("{")
  tabs += 1
  writeline("var src = new TaskCompletionSource<" + t + ">();")
  writeline("completionSources.Add(sequence, src);")
  writeline("return src;")
  tabs -= 1
  writeline("}")
  whitespace()

#
# HandlePacket
#
writeline("public bool HandlePacket(IPacket pkt)")
writeline("{")
tabs += 1
writeline("switch (pkt)")
writeline("{")
tabs += 1
for t in return_packets:
    varname = get_varname()
    writeline("case ResponsePacket_" + t + " " + varname + ": {")
    tabs += 1
    writeline("if (completionSources.TryGetValue(" + varname + ".Sequence, out object k)) {")
    tabs += 1
    writeline("completionSources.Remove(" + varname + ".Sequence);")
    writeline("if (k is TaskCompletionSource<" + t + "> i) i.SetResult(" + varname + ".Value);")
    tabs -= 1
    writeline("}")
    writeline("return true;")
    tabs -= 1
    writeline("}")
tabs -= 1
writeline("}")
writeline("return false;")
tabs -= 1
writeline("}")
tabs -= 1
writeline("}")


#
# Generated Packets
#
writeline("static class GeneratedProtocol")
writeline("{")
tabs += 1
# Register Packets
writeline("public static void RegisterPackets()")
writeline("{")
tabs += 1
def write_register(classname):
    writeline("Packets.Register<" + classname + ">(" + classname + ".Read);")
for t in return_packets:
    write_register("ResponsePacket_" + t)
for mthd in jfile["server_methods"]:
  write_register("ServerPacket_" + mthd["name"])
for mthd in jfile["client_methods"]:
  write_register("ClientPacket_" + mthd["name"])
tabs -= 1
writeline("}")
whitespace()

def handle_packet(mthd, classname):
  global tabs
  varname = get_varname()
  writeline("case " + classname + " " + varname + ": {")
  tabs += 1
  if "return" in mthd:
    writestart("var retval = await player." + mthd["name"] + "(")
  else:
    writestart("player." + mthd["name"] + "(")
  tabs += 1
  if "args" in mthd:
    for idx, a in enumerate(mthd["args"]):
      if idx + 1 < len(mthd["args"]):
        write(varname + "." + a["name"] + ",")
      else:
        write(varname + "." + a["name"])
  tabs -= 1
  writeend(");")
  if "return" in mthd:
    writeline("res.SendResponse(new ResponsePacket_" + mthd["return"] + "() { Sequence = " + varname + ".Sequence, Value = retval });");
  writeline("return true;")
  tabs -= 1
  writeline("}")

# Server Handler
varidx = 0
writeline("public static async Task<bool> HandleServerPacket(IPacket pkt, IServerPlayer player, INetResponder res)")
writeline("{")
tabs += 1
writeline("switch (pkt)")
writeline("{")
tabs += 1
for mthd in jfile["server_methods"]:
  classname = "ServerPacket_" + mthd["name"]
  handle_packet(mthd, classname)
  
tabs -= 1
writeline("}")
writeline("return false;")
tabs -= 1
writeline("}")
whitespace()


# Client Handler
varidx = 0
writeline("public static async Task<bool> HandleClientPacket(IPacket pkt, IClientPlayer player, INetResponder res)")
writeline("{")
tabs += 1
writeline("switch (pkt)")
writeline("{")
tabs += 1
for mthd in jfile["client_methods"]:
  classname = "ClientPacket_" + mthd["name"]
  handle_packet(mthd, classname)
  
tabs -= 1
writeline("}")
writeline("return false;")
tabs -= 1
writeline("}")
whitespace()



tabs -= 1
writeline("}")

#end file

tabs -= 1
writeline("}")
