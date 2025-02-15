# DrippyAL

DrippyAL is a wrapper for OpenAL to simplify audio playback.

## Play 1 sec sine wave

```cs
var sampleRate = 44100;
var frequency = 440;
var data = Enumerable
    .Range(0, sampleRate)
    .Select(t => (short)(32000 * MathF.Sin(2 * MathF.PI * frequency * t / sampleRate)))
    .ToArray();

using (var device = new AudioDevice())
using (var channel = new Channel(device))
using (var clip = new AudioClip(device, sampleRate, 1, data))
{
    channel.Play(clip);

    // Wait until any key is pressed.
    Console.ReadKey();
}
```

## Play streaming sine wave

```cs
var sampleRate = 44100;
var frequency = 440;

using (var device = new AudioDevice())
using (var stream = new AudioStream(device, sampleRate, 1))
{
    var phase = 0F;
    var delta = 2 * MathF.PI * frequency / sampleRate;

    stream.Play(block =>
    {
        for (var t = 0; t < block.Length; t++)
        {
            block[t] = (short)(32000 * MathF.Sin(phase));
            phase = (phase + delta) % (2 * MathF.PI);
        }
    });

    // Wait until any key is pressed.
    Console.ReadKey();
}
```

## MIDI synthesis with [MeltySynth](https://github.com/sinshu/meltysynth/)

```cs
var sampleRate = 44100;
var synthesizer = new Synthesizer("TimGM6mb.sf2", sampleRate);
var sequencer = new MidiFileSequencer(synthesizer);
var midiFile = new MidiFile(@"C:\Windows\Media\flourish.mid");

sequencer.Play(midiFile, true);

using (var device = new AudioDevice())
using (var stream = new AudioStream(device, sampleRate, 2))
{
    stream.Play(data => sequencer.RenderInterleavedInt16(data));

    // Wait until any key is pressed.
    Console.ReadKey();
}
```
