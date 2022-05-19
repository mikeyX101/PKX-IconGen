import bpy
from typing import List

from data.color import Color
from importer import import_hsd


def import_model(model: str):
    import_hsd.load(None, bpy.context, model, 0, "scene_data", "SCENE", True, True, 1000, True)


# TODO See for mix node, factor is always at 1. use that to remove the shiny color? see if factor 1 is a bug and was supposed to be at 0.5
def apply_color_mat(color: Color):
    mats = bpy.data.materials
    for mat in mats:
        tree = mat.node_tree
        if tree is not None:
            try:
                rgb_node = tree.nodes["RGB"]
                rgb_node.outputs[0].default_value = color.to_list_alpha()
            except KeyError:
                mix_node = tree.nodes["TEX_COLORMAP_BLEND 1.0"]
                rgb_node = tree.nodes.new("ShaderNodeRGB")
                tree.links.new(rgb_node.outputs[0], mix_node.inputs[1])
                rgb_node.outputs[0].default_value = color.to_list_alpha()


def remove_objects(removed_objects: List[str]):
    objs = bpy.data.objects
    # Show everything
    for obj in objs:
        obj.hide_render = False
        obj.hide_viewport = False

    # and hide back the removed objects
    for obj_name in removed_objects:
        obj = objs[obj_name]
        obj.hide_render = True
        obj.hide_viewport = True
