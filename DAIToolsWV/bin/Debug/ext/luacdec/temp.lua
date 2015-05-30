-- Decompiled using luadec 2.0 standard by sztupy (http://luadec51.luaforge.net)
-- Command line was: temp.luac 

compiledlua_33c5d69c_4de4_4713_86bc_2a165b9fb4fa = function(l_1_0, l_1_1, l_1_2, l_1_3, l_1_4)
  valid = true
  if l_1_0 == nil then
    LuaError("No TimelineContext for compute_force_script", false)
    print("timeline context not valid")
    valid = false
  else
    print("timeline context valid")
  end
  force = l_1_2
  if force < 0 and valid then
    force = DA3.GetAbilityProperty(l_1_0, l_1_1)
    if force == nil then
      force = 0
    end
    print("ability force")
  end
  if force <= 0 then
    force = 0.15
    print("default force")
  end
  print("base force = " .. DA3.ToString(force))
  if valid then
    sourceCharacter = DA3.GetPartnerCharacter(l_1_0)
    print("source = " .. DA3.ToString(sourceCharacter))
    if l_1_3 ~= 0 then
      print("tag = " .. DA3.ToString(l_1_3))
      if DA3.CharacterHasTimelineTag(sourceCharacter, l_1_3) then
        force = force + l_1_4
        print("modified force = " .. DA3.ToString(force))
      end
    end
  end
  force = math.min(force, 1)
  print("final force = " .. DA3.ToString(force))
  return force
end


