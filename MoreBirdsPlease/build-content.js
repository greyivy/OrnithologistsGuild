const { promises: fs } = require("fs");
const path = require("path");

const uniqueId = "Ivy.MoreBirdsPlease";

(async () => {
    console.log('Building content.json... ')

    const feeders = JSON.parse(await fs.readFile("feeders.json", "utf-8"));
    const foods = JSON.parse(await fs.readFile("foods.json", "utf-8"));

    const output = [
        {
            "$ItemType": "Object",
            "ID": "LifeList",
            "Texture": "LifeList.png:0",
            "SellPrice": null,
            "CanTrash": false,
            "IsGiftable": false,
            "Category": "Junk"
        },
        {
            "$ItemType": "Object",
            "ID": "Binoculars",
            "Texture": "Binoculars.png:0",
            "SellPrice": 100,
            "Category": "Junk"
        },
        {
            "$ItemType": "ShopEntry",
            "Item": {
                "Type": "DGAItem",
                "Value": "Ivy.MoreBirdsPlease/Binoculars"
            },
            "ShopId": "TravelingMerchant",
            "MaxSold": 1,
            "Cost": 250
        },
        {
            "$ItemType": "Object",
            "ID": "HulledSunflowerSeeds",
            "Texture": "HulledSunflowerSeeds.png:0",
            "SellPrice": 20,
            "Category": "Seeds"
        },
        {
            "$ItemType": "BigCraftable",
            "ID": "SeedHuller",
            "Texture": "SeedHuller.png:0",
            "SellPrice": 500
        },
        {
            "$ItemType": "ShopEntry",
            "Item": {
                "Type": "DGAItem",
                "Value": "Ivy.MoreBirdsPlease/SeedHuller"
            },
            "ShopId": "AnimalSupplies",
            "MaxSold": 1,
            "Cost": 2500
        },
        {
            "$ItemType": "MachineRecipe",
            "MachineId": "Ivy.MoreBirdsPlease/SeedHuller",
            "MinutesToProcess": 30,
            "MachineWorkingTextureOverride": "SeedHuller.png:1",
            "MachinePulseWhileWorking": true,
            "Ingredients": [
                {
                    "Type": "VanillaObject",
                    "Value": "Sunflower Seeds",
                    "Quantity": 1
                }
            ],
            "Result": [
                {
                    "Weight": 1,
                    "Value": {
                        "Type": "DGAItem",
                        "Value": "Ivy.MoreBirdsPlease/HulledSunflowerSeeds"
                    }
                }
            ]
        },
    ];

    for (let feeder of feeders) {
        // BigCraftable
        output.push({
            "$ItemType": "BigCraftable",
            "ID": feeder.id,
            "Texture": `${feeder.texture}:0`,
            "SellPrice": feeder.sellPrice
        })

        // ShopEntry
        output.push({
            "$ItemType": "ShopEntry",
            "Item": {
                "Type": "DGAItem",
                "Value": `${uniqueId}/${feeder.id}`
            },
            "ShopId": "AnimalSupplies",
            "MaxSold": 1,
            "Cost": feeder.cost
        })

        // MachineRecipe
        for (let food of foods) {
            if (food.feedersTypes.includes(feeder.type)) {
                for (let item of food.items) {
                    output.push({
                        "$ItemType": "MachineRecipe",
                        "MachineId": `${uniqueId}/${feeder.id}`,
                        "MinutesToProcess": feeder.capacityHrs * 60,
                        "MachineWorkingTextureOverride": `${feeder.texture}:${food.feederAssetIndex}`,
                        "MachinePulseWhileWorking": false,
                        "StartWorkingSound": null,
                        "Ingredients": [
                            {
                                "Type": item.startsWith("DGA:") ? "DGAItem" : "VanillaObject",
                                "Value": item.startsWith("DGA:") ? item.split(":")[1] : item,
                                "Quantity": 1
                            }
                        ],
                        "Result": [
                            {
                                "Weight": 25,
                                "Value": {
                                    "Type": "VanillaObject",
                                    "Value": "Rotten Plant"
                                }
                            },
                            {
                                "Weight": 3,
                                "Value": {
                                    "Type": "VanillaObject",
                                    "Value": "Iron Ore"
                                }
                            },
                            {
                                "Weight": 1,
                                "Value": {
                                    "Type": "VanillaObject",
                                    "Value": "Gold Ore"
                                }
                            }
                        ]
                    })
                }
            }
        }
    }

    await fs.writeFile(path.join('assets', 'dga', 'content.json'), JSON.stringify(output, null, 2));

    console.log('Done!')
})()
