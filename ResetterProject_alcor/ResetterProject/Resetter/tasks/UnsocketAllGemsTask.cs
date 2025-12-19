using DreamPoeBot.Loki.Game;
using Resetter.Extensions;
using System.Linq;
using System.Threading.Tasks;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.Objects;
using log4net;
using static DreamPoeBot.Loki.Game.LokiPoe.InGameState;
using DreamPoeBot.Loki.Game.GameData;
using System.Windows.Forms;

namespace Resetter.tasks
{
    public class UnsocketAllGemsTask : ITask
    {
        public static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private bool _forceUnsocketGems;
        private bool _forceSocketGemsIntoItem;
        private bool _isWeaponSwapped;
        private string _originalWeaponSet;
        private bool _resetLoop;
        private bool _socketGemsToBodyArmour;
        private bool _isChestSocketed;
        public string Author => "Alcor75";
        public string Description => "Task for removing gems.";
        public string Name => "UnsocketAllGemsTask";
        public string Version => "1.0";


        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }
        public MessageResult Message(DreamPoeBot.Loki.Bot.Message message)
        {
            if (message.Id == Messages.UNSOCKET_GEMS)
            {
                Log.Info("Start unsocket all gems. Part 0");
                _forceUnsocketGems = true;
               
                

                

                return MessageResult.Processed;

            }else if(message.Id == Messages.SOCKET_ALL_GEMS_INTO_ITEMS)
            {
                Log.Info("Start socket all gems into item. Part 0");
                _forceSocketGemsIntoItem = true;





                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
        }
        public static async Task<bool> RemoveAllGemsFromItem(InventoryControlWrapper control)
        {
            await CursorHelper.OpenInventory(true);
            // Log.Info("Openning inventory");
            var skippedGemsCount = 0;
            var thisItem = control.Inventory?.Items.FirstOrDefault();
            while (true)
            {
                
                
                if (thisItem == null)
                {
                    break;
                }
                
                var count = thisItem.SocketedGems.Count();
                
                Log.Info($"Show item name: {thisItem.FullName.ToString()}");

               
                var index = -1;
                if(count == 0)
                {
                    break;
                }
               

                for (int i = 0; i < count; i++)
                {

                    
                    index++;
                    
                    Log.Info($"Show skipped gem count: {skippedGemsCount}");
                    Log.Info($"Show i : {i}");
                    var gemOldIndex = index;
                   
                    if (thisItem == null) break;

                    var gems = thisItem.SocketedGems;
                    if (gems == null ) break;

                    var gem = gems[i];
                    if (gem == null) { continue; }

                    if (gem.Name == "Whirling Blades") { skippedGemsCount++; continue; }
                    if (gem.Name == "Faster Attacks Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Dash") { skippedGemsCount++; continue; }
                    if (gem.Name == "Arrogance Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Vitality") { skippedGemsCount++; continue; }
                    if (gem.Name == "Sniper Mark") { skippedGemsCount++; continue; }
                    if (gem.Name == "Mark On Hit Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Awakened Fork Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Fork Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Kinetic Blast") { skippedGemsCount++; continue; }
                    if (gem.Name == "Awakened Increase Area Of Effect Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Greater Multiple Projectiles") { skippedGemsCount++; continue; }
                    if (gem.Name == "Awakened Greater Multiple Projectiles Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Greater Multiple Projectiles Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Returning Projectiles Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Volatality Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Mark On Hit") { skippedGemsCount++; continue; }
                    if (gem.Name == "Sniper's Mark") { skippedGemsCount++; continue; }
                    if (gem.Name == "Trinity Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Portal") { skippedGemsCount++; continue; }
                    if (gem.Name == "Summon Flame Golem") { skippedGemsCount++; continue; }
                    if (gem.Metadata.Contains("Metadata/Items/Gems/SkillGemHeraldOfAsh")) { skippedGemsCount++; continue; }
                    if (gem.Metadata.Contains("Metadata/Items/Gems/SkillGemHeraldOfPurity")) { skippedGemsCount++; continue; }
                    if (gem.Metadata.Contains("Metadata/Items/Gems/SupportGemIncreasedAreaOfEffectPlus")) { skippedGemsCount++; continue; }
                    if (gem.Name == "Divine Blessing") { skippedGemsCount++; continue; }
                    if (gem.Name == "Inspiration Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Generosity Support") { skippedGemsCount++; continue; }
                  
                    if (gem.Name == "Vaal Discipline") { skippedGemsCount++; continue; }
                    if (gem.Name == "Vaal Haste") { skippedGemsCount++; continue; }
                    if (gem.Name == "Vaal Grace") { skippedGemsCount++; continue; }
                    if (gem.Name == "Discipline") { skippedGemsCount++; continue; }
                    if (gem.Name == "Haste") { skippedGemsCount++; continue; }
                    if (gem.Name == "Grace") { skippedGemsCount++; continue; }
                    if (gem.Name == "Hatred") { skippedGemsCount++; continue; }
                    if (gem.Name == "Wrath") { skippedGemsCount++; continue; }
                    if (gem.Name == "Anger") { skippedGemsCount++; continue; }
                    if (gem.Name == "Precision") { skippedGemsCount++; continue; }
                    if (gem.Name == "Purity of Elements") { skippedGemsCount++; continue; }
                    if (gem.Name == "Purity of Fire") { skippedGemsCount++; continue; }
                    if (gem.Name == "Purity of Lightning") { skippedGemsCount++; continue; }
                    if (gem.Name == "Purity of Ice") { skippedGemsCount++; continue; }
                    if (gem.Name == "Zealotry") { skippedGemsCount++; continue; }
                    if (gem.Name == "Determination") { skippedGemsCount++; continue; }
                    if (gem.Name == "Clarity") { skippedGemsCount++; continue; }
                    if (gem.Name == "Vaal Clarity") { skippedGemsCount++; continue; }
                    if (gem.Name == "Murderous Eye Jewel") { skippedGemsCount++; continue; }
                    if (gem.Name == "Tecrod's Gaze") { skippedGemsCount++; continue; }
                    if (gem.Name == "Searching Eye Jewel") { skippedGemsCount++; continue; }
                    if (gem.Name == "Hypnotic Eye Jewel") { skippedGemsCount++; continue; }
                    if (gem.Name == "Ghastly Eye Jewel") { skippedGemsCount++; continue; }
                    if (gem.Name == "Enlighten") { skippedGemsCount++; continue; }
                    if (gem.Name == "Enlighten Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Whirling Blade") { skippedGemsCount++; continue; }
                    if (gem.Name == "Dash") { skippedGemsCount++; continue; }
                    if (gem.Name == "Increase Critical Damage Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Flame Golem") { skippedGemsCount++; continue; }
                    if (gem.Name == "Flame Dash") { skippedGemsCount++; continue; }
                   
                    if (gem.Name == "Pride") { skippedGemsCount++; continue; }
                    if (gem.Name == "Temporal Rift") { skippedGemsCount++; continue; }
                    if (gem.Name == "Herald Of Thunder") { skippedGemsCount++; continue; }
                    if (gem.Name == "Herald Of Ice") { skippedGemsCount++; continue; }
                    if (gem.Name == "Greater Multiple Projectiles") { skippedGemsCount++; continue; }
                    if (gem.Name == "Awakened Greater Multiple Projectiles Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Greater Multiple Projectiles Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Returning Projectiles Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Volatality Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Mark On Hit") { skippedGemsCount++; continue; }
                    if (gem.Name == "Sniper's Mark") { skippedGemsCount++; continue; }
                    if (gem.Name == "Trinity Support") { skippedGemsCount++; continue; }
                    if (gem.Name == "Portal") { skippedGemsCount++; continue; }




                    var un = control.UnequipSkillGem(gemOldIndex);
                   
                    if (!await Wait.For(() => LokiPoe.InGameState.CursorItemOverlay.Item != null,
                        "Gem to appear on cursor.", 100, 3000))
                    {
                        Log.Info($"Show unsocketting gem : {LokiPoe.InGameState.CursorItemOverlay.Item.Name}");
                        continue;
                    }
                    

                    await CursorHelper.ClearCursorTask();
                   
                    thisItem = control.Inventory?.Items.FirstOrDefault();
                   if (thisItem == null)
                       break;
                    
                }
                if (thisItem.SocketedGems.Count(g => g != null) == skippedGemsCount) break;

            }

            return true;
        }

        public static async Task<bool> SocketAllGemsIntoItem(InventoryControlWrapper control)
        {


            



            await CursorHelper.OpenInventory(true);
           // Log.Info("Openning inventory");
            int gemSocketed = 0;
            

            while (true)
            {


                var thisItem = control.Inventory?.Items.FirstOrDefault();
                if (thisItem == null) break;

                

                var count = thisItem.SocketCount;
                if (count == 0) break;

                if (gemSocketed == count) break;

              
                var index = -1;
                if (count == 0)
                {
                    break;
                }


                for (int i = 0; i < count; i++)
                {
                    var gems = thisItem.SocketedGems;
                    if (gems == null || i >= gems.Count()) break;
                    var newItem = control.Inventory?.Items.FirstOrDefault();
                    var greenGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory?.Items.FirstOrDefault(item => item.SocketColor.ToString() == "Green");
                    var redGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory?.Items.FirstOrDefault(item => item.SocketColor.ToString() == "Red");
                    var blueGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory?.Items.FirstOrDefault(item => item.SocketColor.ToString() == "Blue");
                    var whiteGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Inventory?.Items.FirstOrDefault(item => item.SocketColor.ToString() == "Green" || item.SocketColor.ToString() == "Blue" || item.SocketColor.ToString() == "Red");
                    InventoryControlWrapper mainWrapper = LokiPoe.InGameState.InventoryUi.InventoryControl_Main;
                    //if (thisItem.SocketedGems.Count(g => g != null) == count) return false;
                    index++;
                   
                    if (thisItem.SocketedGems[i] != null)
                    {
                        gemSocketed++;
                        continue;

                    }
                    if (thisItem.SocketColors[i].ToString() == "Green")
                    {
                        if (greenGem == null)
                        {
                            gemSocketed++;
                            continue;
                        }
                      //  Log.Info($"Socket {i} is full , now will find next empty socket to fill ");



                        mainWrapper.Pickup(greenGem.LocalId);
                        await Wait.SleepSafe(550, 1050);

                        // var pickedUpGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Pickup(greenGem.Id, true);
                        var eqip = control.EquipSkillGem(newItem.LocalId, i);
                     //   Log.Info($"Show gem name: {greenGem.FullName}");
                        gemSocketed++;
                        await Wait.SleepSafe(550, 1050);
                        continue;



                    }
                    else if (thisItem.SocketColors[i].ToString() == "Red")
                    {
                        if (redGem == null)
                        {
                            gemSocketed++;
                            continue;
                        }

                       // Log.Info($"Socket {i} is Red , now find a Red gem in inventory and put it into index i of thisItem ");
                        mainWrapper.Pickup(redGem.LocalId);
                        await Wait.SleepSafe(550, 1050);

                        // var pickedUpGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Pickup(greenGem.Id, true);
                        var eqip = control.EquipSkillGem(newItem.LocalId, i);
                   //     Log.Info($"Show gem name: {redGem.FullName}");
                        gemSocketed++;
                        await Wait.SleepSafe(250 , 450);
                        continue;
                    }
                    else if (thisItem.SocketColors[i].ToString() == "Blue")

                    {
                        if (blueGem == null)
                        {
                            gemSocketed++;
                            continue;
                        }
                     //   Log.Info($"Socket {i} is Blue, now find a Blue gem in inventory and put it into index i of thisItem ");
                        mainWrapper.Pickup(blueGem.LocalId);
                        await Wait.SleepSafe(550, 1050);

                        // var pickedUpGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Pickup(greenGem.Id, true);
                        var eqip = control.EquipSkillGem(newItem.LocalId, i);

                        gemSocketed++;
                        await Wait.SleepSafe(550, 1050);
                        continue;
                    }
                    else if (thisItem.SocketColors[i].ToString() == "White")
                    {
                        if (whiteGem == null)
                        {
                            gemSocketed++;
                            continue;
                        }
                     //   Log.Info($"Socket {i} is White, now find a gem in inventory and put it into index i of thisItem ");
                        mainWrapper.Pickup(whiteGem.LocalId);
                        await Wait.SleepSafe(550, 1050);

                        // var pickedUpGem = LokiPoe.InGameState.InventoryUi.InventoryControl_Main.Pickup(greenGem.Id, true);
                        var eqip = control.EquipSkillGem(newItem.LocalId, i);
                        gemSocketed++;
                        await Wait.SleepSafe(550, 1050);
                        continue;
                    }

                    /* foreach (var socket in thisItem.SocketColors)
                     {
                         if(socket)
                     }*/






                    await CursorHelper.ClearCursorTask();
                    // if (index+skippedGemsCount > count-1 )return true;
                    thisItem = control.Inventory?.Items.FirstOrDefault();
                    if (thisItem == null)
                        break;
                    
                }
            }

            //finished socketing, now check weapon swap
            //dont swap if already swapped one time

           
            //weapon swapped successfully
           
            return true;
        
                



                //weapon swapped successfully


            
            
            
        }

        public void Start()
        {
            _forceUnsocketGems = false;
            _forceSocketGemsIntoItem = false;
            _isWeaponSwapped = false;
            _originalWeaponSet =  LokiPoe.InstanceInfo.WeaponSet.ToString();
            _resetLoop = false;
            _socketGemsToBodyArmour = false; 
            _isChestSocketed = false;
        }

        public void Stop()
        {
        }


        public async Task<bool> Run()
        {
            
            if (!_forceUnsocketGems && !_forceSocketGemsIntoItem)
            {
                return false;
            }

            else if (_forceUnsocketGems)
            {


                _forceUnsocketGems = false;

                
                
                    do
                    {
                        var meEquippedItem = LokiPoe.Me.EquippedItems.ToList();
                        foreach (var it in meEquippedItem)
                        {
                            var control = GetInventoryByItem(it);
                            if (control == null) continue;
                            if (control.Inventory == null) continue;
                            if (control.Inventory.Items == null) continue;
                       
                           // if (control == LokiPoe.InGameState.InventoryUi.InventoryControl_TherdRing) continue;
                           
                                // Unsoket all gems.
                           

                            await RemoveAllGemsFromItem(control);
                            if ((control == LokiPoe.InGameState.InventoryUi.InventoryControl_PrimaryOffHand || control == LokiPoe.InGameState.InventoryUi.InventoryControl_SecondaryOffHand) && _isWeaponSwapped == false)
                            {



                                LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.weapon_swap);
                                await Wait.Sleep(20);
                                Log.WarnFormat("weapon swap!  ");
                                var newWeaponSet = LokiPoe.InstanceInfo.WeaponSet.ToString();
                                if (newWeaponSet == _originalWeaponSet)
                                {
                                    Log.Info("weapon swap bugged, try again ");
                                    LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.weapon_swap);
                                    await Wait.Sleep(20);
                                }
                                _isWeaponSwapped = true;

                                break;

                            }
                           
                            



                        }
                        if (_isWeaponSwapped == true && _resetLoop == false)
                        {
                            _resetLoop = true;
                            continue;
                        }
                        if (_resetLoop == true)
                        {
                            break;
                        }
                    } while (_isWeaponSwapped);
               


                _resetLoop = false;
                _isWeaponSwapped = false;

                return true;
            }
            
            else if (_forceSocketGemsIntoItem)
            {
                _forceSocketGemsIntoItem = false ;
                do
                {



                    do
                    {
                        var meEquippedItem = LokiPoe.Me.EquippedItems;
                        foreach (var it in meEquippedItem)
                        {
                            var control = GetInventoryByItem(it);
                            if (control == null) continue;
                            if (control.Inventory == null) continue;
                            if (control.Inventory.Items == null) continue;
                           
                            
                            if (control == LokiPoe.InGameState.InventoryUi.InventoryControl_Chest && _socketGemsToBodyArmour == false)
                            {
                                Log.Info("Body armour detected, skip socketing on body for now");
                                _socketGemsToBodyArmour = true;
                                continue;
                            }
                            if(control == LokiPoe.InGameState.InventoryUi.InventoryControl_Chest && _socketGemsToBodyArmour == true){
                                Log.Info("Body armour detected the 2nd time, now socketing to body armour");
                                await SocketAllGemsIntoItem(control);
                                _socketGemsToBodyArmour = false;
                                break;
                            }

                            if ((control == LokiPoe.InGameState.InventoryUi.InventoryControl_PrimaryOffHand || control == LokiPoe.InGameState.InventoryUi.InventoryControl_SecondaryOffHand) && _isWeaponSwapped == false)
                            {
                                Log.Info($"Start socketting gems to item: {it.FullName} ");
                                await SocketAllGemsIntoItem(control);

                                LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.weapon_swap);
                                await Wait.Sleep(20);
                                Log.WarnFormat("weapon swap!  ");
                                var newWeaponSet = LokiPoe.InstanceInfo.WeaponSet.ToString();
                                if (newWeaponSet == _originalWeaponSet)
                                {
                                    Log.Info("weapon swap bugged, try again ");
                                    LokiPoe.Input.SimulateKeyEvent(LokiPoe.Input.Binding.weapon_swap);
                                    await Wait.Sleep(20);
                                }
                                _isWeaponSwapped = true;

                                break;

                            }
                            // Unsoket all gems.
                            Log.Info($"Start socketting gems to item: {it.FullName} ");
                            // Log.Info($"Show item type: {it.ItemType.ToString()} ");

                            await SocketAllGemsIntoItem(control);

                        }
                        if (_isWeaponSwapped == true && _resetLoop == false)
                        {
                            _resetLoop = true;
                            continue;
                        }
                        if (_resetLoop == true)
                        {
                            break;
                        }


                    } while (_isWeaponSwapped);
                    
                } while (_socketGemsToBodyArmour == true) ;
            //finished socketing all gems into weapon set 1, now weapon swap and socket once more
            //  _isChestSocketed = false;
            //   _socketGemsToBodyArmour = false;
            _resetLoop = false;
                _isWeaponSwapped = false;
                _socketGemsToBodyArmour = false;

                return true;
            }
            _resetLoop = false;
            _isWeaponSwapped = false;

           
            await Coroutines.CloseBlockingWindows();
            await Coroutines.LatencyWait();

            return true;
        }

        public void Tick()
        {
        }

        public static InventoryControlWrapper GetInventoryByItem(Item item)
        {
            return LokiPoe.InGameState.InventoryUi.AllInventoryControls.FirstOrDefault(x => x.Inventory.Items.Contains(item));
        }

        public MessageResult MessageMessege(DreamPoeBot.Loki.Bot.Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}
