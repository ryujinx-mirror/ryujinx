package org.ryujinx.android.viewmodels;

import com.sun.jna.Structure;

import java.util.List;


public class GameInfo extends Structure {
    public double FileSize = 0.0;
    public String TitleName;
    public String TitleId;
    public String Developer;
    public String Version;
    public String Icon;

    @Override
    protected List<String> getFieldOrder() {
        return List.of("FileSize", "TitleName", "TitleId", "Developer", "Version", "Icon");
    }
}