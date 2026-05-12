using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;

if (args.Length < 3)
{
    Console.WriteLine("Usage: PastimeReadingPatcher <input.dll> <translations.json> <output.dll>");
    return 1;
}

var inputPath = args[0];
var jsonPath = args[1];
var outputPath = args[2];

var json = File.ReadAllText(jsonPath);
var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
           ?? throw new Exception("translations.json parse failed");

Console.WriteLine($"[+] {dict.Count} translation entries loaded.");
Console.WriteLine($"[+] Reading {inputPath}");

var asm = AssemblyDefinition.ReadAssembly(inputPath, new ReaderParameters { ReadWrite = false });

int ldstrHits = 0;
int attrHits = 0;
var seenMissing = new HashSet<string>();

void TryReplaceLiteral(ref string s, string ctx)
{
    if (dict.TryGetValue(s, out var zh))
    {
        s = zh;
    }
}

foreach (var module in asm.Modules)
{
    foreach (var type in module.GetTypes())
    {
        // Custom attributes on type
        PatchAttrs(type.CustomAttributes, $"type {type.FullName}");

        foreach (var field in type.Fields)
            PatchAttrs(field.CustomAttributes, $"field {type.FullName}::{field.Name}");

        foreach (var prop in type.Properties)
            PatchAttrs(prop.CustomAttributes, $"prop {type.FullName}::{prop.Name}");

        foreach (var method in type.Methods)
        {
            PatchAttrs(method.CustomAttributes, $"method {type.FullName}::{method.Name}");

            if (!method.HasBody) continue;
            foreach (var instr in method.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldstr && instr.Operand is string s)
                {
                    if (dict.TryGetValue(s, out var zh))
                    {
                        instr.Operand = zh;
                        ldstrHits++;
                    }
                }
            }
        }
    }
}

void PatchAttrs(Mono.Collections.Generic.Collection<CustomAttribute> attrs, string ctx)
{
    foreach (var attr in attrs)
    {
        // Constructor args
        for (int i = 0; i < attr.ConstructorArguments.Count; i++)
        {
            var arg = attr.ConstructorArguments[i];
            var newArg = PatchAttrArg(arg);
            if (!ReferenceEquals(arg.Value, newArg.Value))
                attr.ConstructorArguments[i] = newArg;
        }
        // Properties + fields (named)
        for (int i = 0; i < attr.Properties.Count; i++)
        {
            var p = attr.Properties[i];
            var newArg = PatchAttrArg(p.Argument);
            if (!ReferenceEquals(p.Argument.Value, newArg.Value))
                attr.Properties[i] = new CustomAttributeNamedArgument(p.Name, newArg);
        }
        for (int i = 0; i < attr.Fields.Count; i++)
        {
            var f = attr.Fields[i];
            var newArg = PatchAttrArg(f.Argument);
            if (!ReferenceEquals(f.Argument.Value, newArg.Value))
                attr.Fields[i] = new CustomAttributeNamedArgument(f.Name, newArg);
        }
    }
}

CustomAttributeArgument PatchAttrArg(CustomAttributeArgument arg)
{
    if (arg.Value is string s && dict.TryGetValue(s, out var zh))
    {
        attrHits++;
        return new CustomAttributeArgument(arg.Type, zh);
    }
    if (arg.Value is CustomAttributeArgument[] arr)
    {
        bool changed = false;
        var newArr = new CustomAttributeArgument[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            newArr[i] = PatchAttrArg(arr[i]);
            if (!ReferenceEquals(newArr[i].Value, arr[i].Value)) changed = true;
        }
        if (changed)
            return new CustomAttributeArgument(arg.Type, newArr);
    }
    return arg;
}

Console.WriteLine($"[+] ldstr replacements: {ldstrHits}");
Console.WriteLine($"[+] custom-attribute string replacements: {attrHits}");

Console.WriteLine($"[+] Writing {outputPath}");
asm.Write(outputPath);
Console.WriteLine("[+] Done.");
return 0;
