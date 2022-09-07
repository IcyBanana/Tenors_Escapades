//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float3 _SkyGradient[50];

void SampleGradient_float(half tod, out float3 colorA, out float3 colorB) {
    tod *= 50;
    int todInt = floor(tod);
    if(todInt >= 50)
        todInt = 0;
    int nextTodInt = todInt + 1;
    if(nextTodInt >= 50)
        nextTodInt = 0;
    
    colorA = _SkyGradient[todInt];
    colorB = _SkyGradient[nextTodInt];
}

#endif //MYHLSLINCLUDE_INCLUDED