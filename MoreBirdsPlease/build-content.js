const { promises: fs } = require("fs");
const path = require("path");

const uniqueId = "Ivy.MoreBirdsPlease";

(async () => {
    console.log('Building content.json... ')

    const feeders = JSON.parse(await fs.readFile("feeders.json", "utf-8"));
    const foods = JSON.parse(await fs.readFile("foods.json", "utf-8"));

    const output = [];

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
                                "Type": "VanillaObject",
                                "Value": item,
                                "Quantity": 1
                            }
                        ],
                        "Result": [
                            {
                                "Weight": 20,
                                "Value": {
                                    "Type": "VanillaObject",
                                    "Value": "Rotten Plant"
                                }
                            },
                            {
                                "Weight": 5,
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
